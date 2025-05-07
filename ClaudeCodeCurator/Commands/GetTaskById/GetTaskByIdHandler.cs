namespace ClaudeCodeCurator.Commands.GetTaskById;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdRequest, Result<TaskModel>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GetTaskByIdHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<TaskModel>> Handle(GetTaskByIdRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Find the task by ID and include the user story data
            var taskData = await context.Tasks
                .AsNoTracking()
                .Join(
                    context.UserStories.AsNoTracking(),
                    task => task.UserStoryId,
                    userStory => userStory.Id,
                    (task, userStory) => new { Task = task, UserStory = userStory }
                )
                .FirstOrDefaultAsync(x => x.Task.Id == request.TaskId, cancellationToken);

            if (taskData == null)
            {
                return Result.Fail($"Task with ID '{request.TaskId}' not found");
            }

            // Map to model
            var taskModel = new TaskModel
            {
                Id = taskData.Task.Id,
                Name = taskData.Task.Name,
                PromptBody = taskData.Task.PromptBody,
                TaskNumber = taskData.Task.TaskNumber,
                Type = taskData.Task.Type,
                UserStoryId = taskData.Task.UserStoryId,
                UserStoryNumber = taskData.UserStory.UserStoryNumber,
                ApprovedByUserUtc = taskData.Task.ApprovedByUserUtc,
                RequestedByAiUtc = taskData.Task.RequestedByAiUtc,
                FinishedByAiUtc = taskData.Task.FinishedByAiUtc,
                CreatedOrUpdatedUtc = taskData.Task.CreatedOrUpdatedUtc,
                Paused = taskData.Task.Paused
            };

            return Result.Ok(taskModel);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve task: {ex.Message}");
        }
    }
}