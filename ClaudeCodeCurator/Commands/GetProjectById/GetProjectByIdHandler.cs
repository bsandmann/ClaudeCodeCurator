namespace ClaudeCodeCurator.Commands.GetProjectById;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdRequest, Result<ProjectModel>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GetProjectByIdHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<ProjectModel>> Handle(GetProjectByIdRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Start building the query
            IQueryable<Entities.ProjectEntity> query = context.Projects
                .AsNoTracking()
                .Where(p => p.Id == request.ProjectId);
            
            // Build query based on inclusion flags
            if (!request.ExcludeUserStories)
            {
                // Include user stories
                if (!request.ExcludeTasks)
                {
                    // Include both user stories and their tasks
                    query = query.Include(p => p.UserStories)
                                 .ThenInclude(us => us.Tasks);
                }
                else
                {
                    // Include user stories only
                    query = query.Include(p => p.UserStories);
                }
            }
            
            // Execute the query
            var projectEntity = await query.FirstOrDefaultAsync(cancellationToken);

            if (projectEntity == null)
            {
                return Result.Fail($"Project with ID '{request.ProjectId}' not found");
            }

            // Create the base project model
            var projectModel = new ProjectModel
            {
                Id = projectEntity.Id,
                Name = projectEntity.Name,
                PrimePrompt = projectEntity.PrimePrompt,
                UserStoryNumberCounter = projectEntity.UserStoryNumberCounter,
                TaskNumberCounter = projectEntity.TaskNumberCounter,
                CreatedOrUpdatedUtc = projectEntity.CreatedOrUpdatedUtc,
                UserStories = new List<UserStoryModel>()
            };
            
            // Map user stories if they were included
            if (!request.ExcludeUserStories && projectEntity.UserStories != null)
            {
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
                    
                    // Map tasks if they were included
                    if (!request.ExcludeTasks && userStoryEntity.Tasks != null)
                    {
                        foreach (var taskEntity in userStoryEntity.Tasks)
                        {
                            userStoryModel.Tasks.Add(new TaskModel
                            {
                                Id = taskEntity.Id,
                                Name = taskEntity.Name,
                                PromptBody = taskEntity.PromptBody,
                                TaskNumber = taskEntity.TaskNumber,
                                Type = taskEntity.Type,
                                ReferenceUserStory = taskEntity.ReferenceUserStory,
                                PromptAppendThink = taskEntity.PromptAppendThink,
                                PromptAppendThinkHard = taskEntity.PromptAppendThinkHard,
                                PromptAppendDoNotChange = taskEntity.PromptAppendDoNotChange,
                                UserStoryId = userStoryEntity.Id,
                                ApprovedByUserUtc = taskEntity.ApprovedByUserUtc,
                                RequestedByAiUtc = taskEntity.RequestedByAiUtc,
                                FinishedByAiUtc = taskEntity.FinishedByAiUtc,
                                CreatedOrUpdatedUtc = taskEntity.CreatedOrUpdatedUtc,
                                Paused = taskEntity.Paused
                            });
                        }
                    }
                    
                    projectModel.UserStories.Add(userStoryModel);
                }
            }

            return Result.Ok(projectModel);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve project: {ex.Message}");
        }
    }
}