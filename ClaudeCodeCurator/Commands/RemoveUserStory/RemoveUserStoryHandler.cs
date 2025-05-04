namespace ClaudeCodeCurator.Commands.RemoveUserStory;

using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class RemoveUserStoryHandler : IRequestHandler<RemoveUserStoryRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RemoveUserStoryHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(RemoveUserStoryRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Find the user story by ID, include tasks if needed
            var query = context.UserStories.AsTracking().AsQueryable();
            
            if (!request.CascadeTasks)
            {
                // If not cascading, we need to check if tasks exist
                query = query.Include(us => us.Tasks);
            }
            
            var userStory = await query
                .FirstOrDefaultAsync(us => us.Id == request.UserStoryId, cancellationToken);

            if (userStory == null)
            {
                return Result.Fail($"User story with ID '{request.UserStoryId}' not found");
            }

            // If not cascading and there are associated tasks, return an error
            if (!request.CascadeTasks && userStory.Tasks != null && userStory.Tasks.Count > 0)
            {
                return Result.Fail($"Cannot remove user story with ID '{request.UserStoryId}' because it has associated tasks. Set cascadeTasks to true to remove these tasks as well.");
            }

            // If cascading, we need to explicitly remove tasks
            if (request.CascadeTasks)
            {
                // Load tasks if they weren't already loaded
                if (userStory.Tasks == null)
                {
                    var tasks = await context.Tasks
                        .Where(t => t.UserStoryId == request.UserStoryId)
                        .ToListAsync(cancellationToken);
                        
                    // Remove each task
                    if (tasks.Count > 0)
                    {
                        context.Tasks.RemoveRange(tasks);
                    }
                }
                else
                {
                    // Tasks were already loaded with the include above
                    context.Tasks.RemoveRange(userStory.Tasks);
                }
            }

            // Remove the user story
            context.UserStories.Remove(userStory);
            await context.SaveChangesAsync(cancellationToken);
            
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to remove user story: {ex.Message}");
        }
    }
}