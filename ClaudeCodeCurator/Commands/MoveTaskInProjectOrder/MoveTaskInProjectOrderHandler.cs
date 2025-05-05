namespace ClaudeCodeCurator.Commands.MoveTaskInProjectOrder;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class MoveTaskInProjectOrderHandler : IRequestHandler<MoveTaskInProjectOrderRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MoveTaskInProjectOrderHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(MoveTaskInProjectOrderRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Start a transaction to ensure consistency
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Check if the task exists and is in the ordered list
                var orderedTask = await context.ProjectTaskOrders
                    .AsTracking()
                    .FirstOrDefaultAsync(o => 
                        o.ProjectId == request.ProjectId && 
                        o.TaskId == request.TaskId, 
                        cancellationToken);
                
                if (orderedTask == null)
                {
                    return Result.Fail($"Task with ID '{request.TaskId}' is not in the ordered list for project '{request.ProjectId}'");
                }
                
                // Handle different position types
                switch (request.PositionType)
                {
                    case PositionType.ToTop:
                        // Get minimum position and place task above it
                        var minPosition = await context.ProjectTaskOrders
                            .Where(o => o.ProjectId == request.ProjectId && o.TaskId != request.TaskId)
                            .MinAsync(o => o.Position, cancellationToken);
                        
                        orderedTask.Position = minPosition > 100 ? minPosition - 100 : 1;
                        break;
                        
                    case PositionType.ToBottom:
                        // Get maximum position and place task below it
                        var maxPosition = await context.ProjectTaskOrders
                            .Where(o => o.ProjectId == request.ProjectId && o.TaskId != request.TaskId)
                            .MaxAsync(o => o.Position, cancellationToken);
                        
                        orderedTask.Position = maxPosition + 100;
                        break;
                        
                    case PositionType.BeforeTask:
                        if (!request.ReferenceTaskId.HasValue)
                        {
                            return Result.Fail("Reference task ID is required when using BeforeTask position type");
                        }
                        
                        // Get reference task
                        var beforeTask = await context.ProjectTaskOrders
                            .FirstOrDefaultAsync(o => 
                                o.ProjectId == request.ProjectId && 
                                o.TaskId == request.ReferenceTaskId.Value, 
                                cancellationToken);
                                
                        if (beforeTask == null)
                        {
                            return Result.Fail($"Reference task with ID '{request.ReferenceTaskId}' not found in the ordered list");
                        }
                        
                        // Find task right before the reference task
                        var taskBeforeReference = await context.ProjectTaskOrders
                            .Where(o => 
                                o.ProjectId == request.ProjectId && 
                                o.Position < beforeTask.Position &&
                                o.TaskId != request.TaskId)
                            .OrderByDescending(o => o.Position)
                            .FirstOrDefaultAsync(cancellationToken);
                        
                        int newPosition;
                        if (taskBeforeReference != null)
                        {
                            // Place in between
                            newPosition = taskBeforeReference.Position + (beforeTask.Position - taskBeforeReference.Position) / 2;
                            
                            // If no space between, need to reorganize
                            if (newPosition == taskBeforeReference.Position)
                            {
                                await ReorganizePositionsAsync(context, request.ProjectId, cancellationToken);
                                
                                // Refresh positions after reorganization
                                beforeTask = await context.ProjectTaskOrders
                                    .FirstAsync(o => 
                                        o.ProjectId == request.ProjectId && 
                                        o.TaskId == request.ReferenceTaskId.Value, 
                                        cancellationToken);
                                
                                taskBeforeReference = await context.ProjectTaskOrders
                                    .Where(o => 
                                        o.ProjectId == request.ProjectId && 
                                        o.Position < beforeTask.Position &&
                                        o.TaskId != request.TaskId)
                                    .OrderByDescending(o => o.Position)
                                    .FirstOrDefaultAsync(cancellationToken);
                                
                                if (taskBeforeReference != null)
                                {
                                    newPosition = taskBeforeReference.Position + (beforeTask.Position - taskBeforeReference.Position) / 2;
                                }
                                else
                                {
                                    newPosition = beforeTask.Position - 100;
                                }
                            }
                        }
                        else
                        {
                            // No task before reference, place at beginning
                            newPosition = beforeTask.Position > 100 ? beforeTask.Position - 100 : 1;
                        }
                        
                        orderedTask.Position = newPosition;
                        break;
                        
                    case PositionType.AfterTask:
                        if (!request.ReferenceTaskId.HasValue)
                        {
                            return Result.Fail("Reference task ID is required when using AfterTask position type");
                        }
                        
                        // Get reference task
                        var afterTask = await context.ProjectTaskOrders
                            .FirstOrDefaultAsync(o => 
                                o.ProjectId == request.ProjectId && 
                                o.TaskId == request.ReferenceTaskId.Value, 
                                cancellationToken);
                                
                        if (afterTask == null)
                        {
                            return Result.Fail($"Reference task with ID '{request.ReferenceTaskId}' not found in the ordered list");
                        }
                        
                        // Find task right after the reference task
                        var taskAfterReference = await context.ProjectTaskOrders
                            .Where(o => 
                                o.ProjectId == request.ProjectId && 
                                o.Position > afterTask.Position &&
                                o.TaskId != request.TaskId)
                            .OrderBy(o => o.Position)
                            .FirstOrDefaultAsync(cancellationToken);
                        
                        if (taskAfterReference != null)
                        {
                            // Place in between
                            int midPosition = afterTask.Position + (taskAfterReference.Position - afterTask.Position) / 2;
                            
                            // If no space between, need to reorganize
                            if (midPosition == afterTask.Position)
                            {
                                await ReorganizePositionsAsync(context, request.ProjectId, cancellationToken);
                                
                                // Refresh positions after reorganization
                                afterTask = await context.ProjectTaskOrders
                                    .FirstAsync(o => 
                                        o.ProjectId == request.ProjectId && 
                                        o.TaskId == request.ReferenceTaskId.Value, 
                                        cancellationToken);
                                
                                taskAfterReference = await context.ProjectTaskOrders
                                    .Where(o => 
                                        o.ProjectId == request.ProjectId && 
                                        o.Position > afterTask.Position &&
                                        o.TaskId != request.TaskId)
                                    .OrderBy(o => o.Position)
                                    .FirstOrDefaultAsync(cancellationToken);
                                
                                if (taskAfterReference != null)
                                {
                                    midPosition = afterTask.Position + (taskAfterReference.Position - afterTask.Position) / 2;
                                }
                                else
                                {
                                    midPosition = afterTask.Position + 100;
                                }
                            }
                            
                            orderedTask.Position = midPosition;
                        }
                        else
                        {
                            // No task after reference, place at end
                            orderedTask.Position = afterTask.Position + 100;
                        }
                        break;
                        
                    case PositionType.ToPosition:
                        if (!request.Position.HasValue)
                        {
                            return Result.Fail("Position value is required when using ToPosition position type");
                        }
                        
                        // Check if position is already taken
                        var existingTaskAtPosition = await context.ProjectTaskOrders
                            .FirstOrDefaultAsync(o => 
                                o.ProjectId == request.ProjectId && 
                                o.Position == request.Position.Value &&
                                o.TaskId != request.TaskId, 
                                cancellationToken);
                        
                        if (existingTaskAtPosition != null)
                        {
                            // Need to reorganize to make room
                            bool shiftUp = orderedTask.Position < request.Position.Value;
                            
                            if (shiftUp)
                            {
                                // Shift tasks up that are between old and new position
                                await context.ProjectTaskOrders
                                    .Where(o => 
                                        o.ProjectId == request.ProjectId && 
                                        o.Position >= request.Position.Value &&
                                        o.TaskId != request.TaskId)
                                    .ForEachAsync(o => o.Position += 100, cancellationToken);
                            }
                            else
                            {
                                // Shift tasks down that are between new and old position
                                await context.ProjectTaskOrders
                                    .Where(o => 
                                        o.ProjectId == request.ProjectId && 
                                        o.Position <= request.Position.Value &&
                                        o.Position > orderedTask.Position &&
                                        o.TaskId != request.TaskId)
                                    .ForEachAsync(o => o.Position -= 100, cancellationToken);
                            }
                        }
                        
                        orderedTask.Position = request.Position.Value;
                        break;
                        
                    default:
                        return Result.Fail("Invalid position type specified");
                }
                
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw; // Re-throw to be caught by outer try-catch
            }
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update task position: {ex.Message}");
        }
    }
    
    // Helper method to reorganize positions with gaps
    private async Task ReorganizePositionsAsync(DataContext context, Guid projectId, CancellationToken cancellationToken)
    {
        // Get all ordered tasks for the project, sorted by position
        var orderedTasks = await context.ProjectTaskOrders
            .Where(o => o.ProjectId == projectId)
            .OrderBy(o => o.Position)
            .ToListAsync(cancellationToken);
        
        // Reassign positions with gaps of 100
        for (int i = 0; i < orderedTasks.Count; i++)
        {
            orderedTasks[i].Position = (i + 1) * 100;
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}