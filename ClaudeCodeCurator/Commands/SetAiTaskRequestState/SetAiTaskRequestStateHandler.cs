namespace ClaudeCodeCurator.Commands.SetAiTaskRequestState;

using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class SetAiTaskRequestStateHandler : IRequestHandler<SetAiTaskRequestStateRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SetAiTaskRequestStateHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(SetAiTaskRequestStateRequest request, CancellationToken cancellationToken)
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
            
            // If requesting by AI, set current UTC time
            // If clearing request, set to null
            DateTime? currentValue = task.RequestedByAiUtc;
            DateTime? newValue = request.RequestedByAi ? DateTime.UtcNow : null;
            
            // Only update if the request status actually changes
            bool statusChanged = (currentValue == null && request.RequestedByAi) || 
                                 (currentValue != null && !request.RequestedByAi);
            
            if (statusChanged)
            {
                task.RequestedByAiUtc = newValue;
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
            return Result.Fail($"Failed to update task AI request status: {ex.Message}");
        }
    }
}