namespace ClaudeCodeCurator.Commands.GetProjectByLastUsed;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class GetProjectByLastUsedHandler : IRequestHandler<GetProjectByLastUsedRequest, Result<ProjectModel>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GetProjectByLastUsedHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<ProjectModel>> Handle(GetProjectByLastUsedRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Find the most recently updated task
            var lastUpdatedTask = await context.Tasks
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedOrUpdatedUtc)
                .FirstOrDefaultAsync(cancellationToken);

            // If there are no tasks in the system, return a failure
            if (lastUpdatedTask == null)
            {
                return Result.Fail("No tasks found in the system");
            }

            // Get the user story for this task
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == lastUpdatedTask.UserStoryId, cancellationToken);

            if (userStory == null)
            {
                return Result.Fail($"User story with ID '{lastUpdatedTask.UserStoryId}' not found");
            }

            // Get the project ID
            var projectId = userStory.ProjectId;

            // Load the complete project with all user stories and tasks
            var projectEntity = await context.Projects
                .AsNoTracking()
                .Include(p => p.UserStories)
                    .ThenInclude(us => us.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

            if (projectEntity == null)
            {
                return Result.Fail($"Project with ID '{projectId}' not found");
            }

            // Map the project entity to a model
            var projectModel = new ProjectModel
            {
                Id = projectEntity.Id,
                Name = projectEntity.Name,
                UserStoryNumberCounter = projectEntity.UserStoryNumberCounter,
                TaskNumberCounter = projectEntity.TaskNumberCounter,
                CreatedOrUpdatedUtc = projectEntity.CreatedOrUpdatedUtc,
                UserStories = new List<UserStoryModel>()
            };

            // Map all user stories and their tasks
            foreach (var userStoryEntity in projectEntity.UserStories)
            {
                var userStoryModel = new UserStoryModel
                {
                    Id = userStoryEntity.Id,
                    Name = userStoryEntity.Name,
                    Description = userStoryEntity.Description,
                    UserStoryNumber = userStoryEntity.UserStoryNumber,
                    CreatedOrUpdatedUtc = userStoryEntity.CreatedOrUpdatedUtc,
                    ProjectId = projectEntity.Id,
                    Tasks = new List<TaskModel>()
                };

                // Add all tasks for this user story
                foreach (var taskEntity in userStoryEntity.Tasks)
                {
                    userStoryModel.Tasks.Add(new TaskModel
                    {
                        Id = taskEntity.Id,
                        Name = taskEntity.Name,
                        PromptBody = taskEntity.PromptBody,
                        TaskNumber = taskEntity.TaskNumber,
                        Type = taskEntity.Type,
                        UserStoryId = userStoryEntity.Id,
                        ApprovedByUserUtc = taskEntity.ApprovedByUserUtc,
                        RequestedByAiUtc = taskEntity.RequestedByAiUtc,
                        FinishedByAiUtc = taskEntity.FinishedByAiUtc,
                        CreatedOrUpdatedUtc = taskEntity.CreatedOrUpdatedUtc,
                        Paused = taskEntity.Paused
                    });
                }

                // Add the user story with its tasks to the project
                projectModel.UserStories.Add(userStoryModel);
            }

            return Result.Ok(projectModel);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve the most recently used project: {ex.Message}");
        }
    }
}