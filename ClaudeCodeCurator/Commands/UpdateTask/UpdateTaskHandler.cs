namespace ClaudeCodeCurator.Commands.UpdateTask;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdateTaskHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(UpdateTaskRequest request, CancellationToken cancellationToken)
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

            // Check if a different task in the same user story has the same name
            var existingTask = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.UserStoryId == task.UserStoryId && 
                         t.Name == request.Name && 
                         t.Id != request.TaskId, 
                    cancellationToken);

            if (existingTask != null)
            {
                return Result.Fail($"A different task with name '{request.Name}' already exists in this user story");
            }

            // Check if any changes are needed (optimization)
            bool hasChanges = false;
            
            if (task.Name != request.Name)
            {
                task.Name = request.Name;
                hasChanges = true;
            }
            
            if (task.PromptBody != request.PromptBody)
            {
                task.PromptBody = request.PromptBody;
                hasChanges = true;
            }
            
            if (task.Type != request.Type)
            {
                task.Type = request.Type;
                hasChanges = true;
            }
            
            // Only save if there are actual changes
            if (hasChanges)
            {
                task.CreatedOrUpdatedUtc = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
                return Result.Ok(true);
            }
            
            return Result.Ok(false); // No changes were needed
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update task: {ex.Message}");
        }
    }
}