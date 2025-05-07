namespace ClaudeCodeCurator.Commands.SetResetTask;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class SetResetTaskHandler : IRequestHandler<SetResetTaskRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SetResetTaskHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(SetResetTaskRequest request, CancellationToken cancellationToken)
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
                    // Find the task by ID with related UserStory
                    var task = await context.Tasks
                        .AsTracking()
                        .Include(t => t.UserStory)
                        .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

                    if (task == null)
                    {
                        return Result.Fail($"Task with ID '{request.TaskId}' not found");
                    }

                    // Check if any changes are needed for the various states
                    bool hasChanges = false;
                    
                    // Check if ApprovedByUserUtc needs to be reset
                    bool wasApproved = task.ApprovedByUserUtc != null;
                    if (wasApproved)
                    {
                        task.ApprovedByUserUtc = null;
                        hasChanges = true;
                    }
                    
                    // Check if RequestedByAiUtc needs to be reset
                    if (task.RequestedByAiUtc != null)
                    {
                        task.RequestedByAiUtc = null;
                        hasChanges = true;
                    }
                    
                    // Check if FinishedByAiUtc needs to be reset
                    if (task.FinishedByAiUtc != null)
                    {
                        task.FinishedByAiUtc = null;
                        hasChanges = true;
                    }
                    
                    // Update timestamp if any changes were made
                    if (hasChanges)
                    {
                        task.CreatedOrUpdatedUtc = DateTime.UtcNow;
                        
                        // If task was approved before, remove it from ProjectTaskOrders
                        if (wasApproved)
                        {
                            // Handle the project ordered list
                            Guid projectId = request.ProjectId ?? task.UserStory.ProjectId;
                            
                            // Remove from ordered list
                            var existingOrder = await context.ProjectTaskOrders
                                .AsTracking()
                                .FirstOrDefaultAsync(o => o.ProjectId == projectId && o.TaskId == task.Id, cancellationToken);
                                
                            if (existingOrder != null)
                            {
                                context.ProjectTaskOrders.Remove(existingOrder);
                            }
                        }
                        
                        // Save all changes
                        await context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return Result.Ok(true);
                    }
                    
                    // If no changes were needed
                    await transaction.CommitAsync(cancellationToken);
                    return Result.Ok(false);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw; // Re-throw to be caught by outer try-catch
                }
            });
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to reset task: {ex.Message}");
        }
    }
}