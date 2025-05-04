namespace ClaudeCodeCurator.Commands.UpdateUserStory;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class UpdateUserStoryHandler : IRequestHandler<UpdateUserStoryRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdateUserStoryHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(UpdateUserStoryRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Find the user story by ID
            var userStory = await context.UserStories
                .FirstOrDefaultAsync(us => us.Id == request.UserStoryId, cancellationToken);

            if (userStory == null)
            {
                return Result.Fail($"User story with ID '{request.UserStoryId}' not found");
            }

            // Check if a different user story in the same project has the same name
            var existingUserStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    us => us.ProjectId == userStory.ProjectId && 
                          us.Name == request.Name && 
                          us.Id != request.UserStoryId, 
                    cancellationToken);

            if (existingUserStory != null)
            {
                return Result.Fail($"A different user story with name '{request.Name}' already exists in this project");
            }

            // Check if any changes are needed (optimization)
            bool hasChanges = false;
            
            if (userStory.Name != request.Name)
            {
                userStory.Name = request.Name;
                hasChanges = true;
            }
            
            // Only update description if it's different from current value
            // Note: Null and empty string are considered different
            bool descriptionChanged = 
                (request.Description == null && userStory.Description != null) ||
                (request.Description != null && userStory.Description != request.Description);
                
            if (descriptionChanged)
            {
                userStory.Description = request.Description;
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
            return Result.Fail($"Failed to update user story: {ex.Message}");
        }
    }
}