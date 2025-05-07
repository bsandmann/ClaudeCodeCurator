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
            // Find the task by ID and include the user story using navigation properties
            var taskEntity = await context.Tasks
                .AsNoTracking()
                .Include(t => t.UserStory)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

            if (taskEntity == null)
            {
                return Result.Fail($"Task with ID '{request.TaskId}' not found");
            }

            // Map to model
            var taskModel = new TaskModel
            {
                Id = taskEntity.Id,
                Name = taskEntity.Name,
                PromptBody = taskEntity.PromptBody,
                TaskNumber = taskEntity.TaskNumber,
                Type = taskEntity.Type,
                UserStoryId = taskEntity.UserStoryId,
                UserStoryNumber = taskEntity.UserStory.UserStoryNumber,
                ApprovedByUserUtc = taskEntity.ApprovedByUserUtc,
                RequestedByAiUtc = taskEntity.RequestedByAiUtc,
                FinishedByAiUtc = taskEntity.FinishedByAiUtc,
                CreatedOrUpdatedUtc = taskEntity.CreatedOrUpdatedUtc,
                Paused = taskEntity.Paused
            };

            return Result.Ok(taskModel);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve task: {ex.Message}");
        }
    }
}