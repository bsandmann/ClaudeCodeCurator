namespace ClaudeCodeCurator.Commands.GetUserStoryById;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class GetUserStoryByIdHandler : IRequestHandler<GetUserStoryByIdRequest, Result<UserStoryModel>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GetUserStoryByIdHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<UserStoryModel>> Handle(GetUserStoryByIdRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Start building the query
            var query = context.UserStories
                .AsNoTracking()
                .Where(us => us.Id == request.UserStoryId);
            
            // Conditionally include tasks based on the flag
            if (!request.ExcludeTasks)
            {
                query = query.Include(us => us.Tasks);
            }
            
            // Execute the query
            var userStoryEntity = await query.FirstOrDefaultAsync(cancellationToken);

            if (userStoryEntity == null)
            {
                return Result.Fail($"User story with ID '{request.UserStoryId}' not found");
            }

            // Create the model
            var userStoryModel = new UserStoryModel
            {
                Id = userStoryEntity.Id,
                Name = userStoryEntity.Name,
                Description = userStoryEntity.Description,
                UserStoryNumber = userStoryEntity.UserStoryNumber,
                ProjectId = userStoryEntity.ProjectId,
                Tasks = new List<TaskModel>()
            };
            
            // If tasks were included, map them
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
                        UserStoryId = userStoryEntity.Id,
                        ApprovedByUserUtc = taskEntity.ApprovedByUserUtc,
                        RequestedByAiUtc = taskEntity.RequestedByAiUtc,
                        FinishedByAiUtc = taskEntity.FinishedByAiUtc
                    });
                }
            }

            return Result.Ok(userStoryModel);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve user story: {ex.Message}");
        }
    }
}