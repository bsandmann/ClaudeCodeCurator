namespace ClaudeCodeCurator.Commands.CreateUserStory;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CreateUserStoryHandler : IRequestHandler<CreateUserStoryRequest, Result<Guid>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateUserStoryHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<Guid>> Handle(CreateUserStoryRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Get the project - we need it to access and update the counter
            var project = await context.Projects
                .AsTracking() // Make sure we're tracking the entity for updates
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (project == null)
            {
                return Result.Fail($"Project with ID '{request.ProjectId}' does not exist");
            }

            // Check if a user story with the same name already exists in the project
            var existingUserStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    us => us.ProjectId == request.ProjectId && us.Name == request.Name, 
                    cancellationToken);

            if (existingUserStory != null)
            {
                return Result.Fail($"User story with name '{request.Name}' already exists in this project");
            }

            // Increment the counter
            project.UserStoryNumberCounter++;
            
            // Create new user story with number
            var userStory = new UserStoryEntity
            {
                Name = request.Name,
                Description = request.Description,
                ProjectId = request.ProjectId,
                UserStoryNumber = project.UserStoryNumberCounter
            };

            await context.UserStories.AddAsync(userStory, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(userStory.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create user story: {ex.Message}");
        }
    }
}