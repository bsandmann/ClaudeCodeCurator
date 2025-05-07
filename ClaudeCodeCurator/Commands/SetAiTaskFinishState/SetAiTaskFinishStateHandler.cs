namespace ClaudeCodeCurator.Commands.SetAiTaskFinishState;

using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class SetAiTaskFinishStateHandler : IRequestHandler<SetAiTaskFinishStateRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SetAiTaskFinishStateHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(SetAiTaskFinishStateRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Find the task by ID
            var task = await context.Tasks
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

            if (task == null)
            {
                return Result.Fail($"Task with ID '{request.TaskId}' not found");
            }

            // Check if any changes are needed (optimization)
            bool hasChanges = false;
            
            // If finishing, set current UTC time
            // If clearing finish state, set to null
            DateTime? currentValue = task.FinishedByAiUtc;
            DateTime? newValue = request.FinishedByAi ? DateTime.UtcNow : null;
            
            // Only update if the finish status actually changes
            bool statusChanged = (currentValue == null && request.FinishedByAi) || 
                                 (currentValue != null && !request.FinishedByAi);
            
            if (statusChanged)
            {
                task.FinishedByAiUtc = newValue;
                task.Paused = false;
                task.CreatedOrUpdatedUtc = DateTime.UtcNow;
                hasChanges = true;
            }
            
            // Only save if there are actual changes
            if (hasChanges)
            {
                await context.SaveChangesAsync(cancellationToken);
                return Result.Ok(true);
            }
            
            return Result.Ok(false); // No changes were needed
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update task finish status: {ex.Message}");
        }
    }
}