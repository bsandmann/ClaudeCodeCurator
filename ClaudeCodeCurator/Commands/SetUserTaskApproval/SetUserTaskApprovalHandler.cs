namespace ClaudeCodeCurator.Commands.SetUserTaskApproval;

using System.Threading;
using System.Threading.Tasks;
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
            return Result.Fail($"Failed to update task approval status: {ex.Message}");
        }
    }
}