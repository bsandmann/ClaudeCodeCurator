namespace ClaudeCodeCurator.Commands.SetTaskPause;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class SetTaskPauseHandler : IRequestHandler<SetTaskPauseRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SetTaskPauseHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(SetTaskPauseRequest request, CancellationToken cancellationToken)
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
            if (task.Paused == request.Paused)
            {
                // No change needed
                return Result.Ok(false);
            }

            // Update pause status and timestamp
            task.Paused = request.Paused;
            task.CreatedOrUpdatedUtc = DateTime.UtcNow;

            // Save changes
            await context.SaveChangesAsync(cancellationToken);
            
            return Result.Ok(true); // Change was made
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update task pause status: {ex.Message}");
        }
    }
}