namespace ClaudeCodeCurator.Commands.RemoveTask;

using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class RemoveTaskHandler : IRequestHandler<RemoveTaskRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RemoveTaskHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(RemoveTaskRequest request, CancellationToken cancellationToken)
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

            // Remove the task
            context.Tasks.Remove(task);
            await context.SaveChangesAsync(cancellationToken);
            
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to remove task: {ex.Message}");
        }
    }
}