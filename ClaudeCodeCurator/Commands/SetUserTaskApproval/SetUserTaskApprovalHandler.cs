namespace ClaudeCodeCurator.Commands.SetUserTaskApproval;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class SetUserTaskApprovalHandler : IRequestHandler<SetUserTaskApprovalRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SetUserTaskApprovalHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(SetUserTaskApprovalRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Create the execution strategy
            var strategy = context.Database.CreateExecutionStrategy();
            
            // Execute the entire operation as a unit with the proper execution strategy
            return await strategy.ExecuteAsync(async () =>
            {
                // Start a transaction to ensure consistency
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    // Find the task by ID with related UserStory and Project
                    var task = await context.Tasks
                        .AsTracking()
                        .Include(t => t.UserStory)
                        .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

                    if (task == null)
                    {
                        return Result.Fail($"Task with ID '{request.TaskId}' not found");
                    }

                    // Check if any changes are needed for approval status (optimization)
                    bool hasChanges = false;
                    
                    // If approving, set current UTC time
                    // If un-approving, set to null
                    DateTime? currentValue = task.ApprovedByUserUtc;
                    DateTime? newValue = request.ApprovedByUser ? DateTime.UtcNow : null;
                    
                    // Only update if the approval status actually changes
                    bool statusChanged = (currentValue == null && request.ApprovedByUser) || 
                                        (currentValue != null && !request.ApprovedByUser);
                    
                    if (statusChanged)
                    {
                        task.ApprovedByUserUtc = newValue;
                        task.CreatedOrUpdatedUtc = DateTime.UtcNow;
                        hasChanges = true;
                        
                        // Handle the project ordered list
                        Guid projectId = request.ProjectId ?? task.UserStory.ProjectId;
                        
                        if (request.ApprovedByUser)
                        {
                            // Add to ordered list if not already there
                            var existingOrder = await context.ProjectTaskOrders
                                .FirstOrDefaultAsync(o => o.ProjectId == projectId && o.TaskId == task.Id, cancellationToken);
                                
                            if (existingOrder == null)
                            {
                                // Get the next position (highest current position + 100)
                                int nextPosition = 100; // Default starting position
                                var highestPosition = await context.ProjectTaskOrders
                                    .Where(o => o.ProjectId == projectId)
                                    .OrderByDescending(o => o.Position)
                                    .Select(o => o.Position)
                                    .FirstOrDefaultAsync(cancellationToken);
                                    
                                if (highestPosition > 0)
                                {
                                    nextPosition = highestPosition + 100; // Leave gaps for future insertions
                                }
                                
                                // Add new entry
                                await context.ProjectTaskOrders.AddAsync(new ProjectTaskOrderEntity
                                {
                                    ProjectId = projectId,
                                    TaskId = task.Id,
                                    Position = nextPosition
                                }, cancellationToken);
                            }
                        }
                        else
                        {
                            // Remove from ordered list when unapproved
                            var existingOrder = await context.ProjectTaskOrders
                                .AsTracking()
                                .FirstOrDefaultAsync(o => o.ProjectId == projectId && o.TaskId == task.Id, cancellationToken);
                                
                            if (existingOrder != null)
                            {
                                context.ProjectTaskOrders.Remove(existingOrder);
                            }
                        }
                    }
                    
                    // Only save if there are actual changes
                    if (hasChanges)
                    {
                        await context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return Result.Ok(true);
                    }
                    
                    await transaction.CommitAsync(cancellationToken);
                    return Result.Ok(false); // No changes were needed
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw; // Re-throw to be caught by outer try-catch
                }
            });
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update task approval status: {ex.Message}");
        }
    }
}