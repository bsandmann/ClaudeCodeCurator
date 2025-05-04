namespace ClaudeCodeCurator.Commands.RemoveProject;

using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class RemoveProjectHandler : IRequestHandler<RemoveProjectRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RemoveProjectHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(RemoveProjectRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Find the project by ID
            var query = context.Projects.AsTracking().AsQueryable();
            
            if (!request.CascadeDelete)
            {
                // If not cascading, we need to check if user stories exist
                query = query.Include(p => p.UserStories);
            }
            
            var project = await query
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (project == null)
            {
                return Result.Fail($"Project with ID '{request.ProjectId}' not found");
            }

            // If not cascading and there are associated user stories, return an error
            if (!request.CascadeDelete && project.UserStories != null && project.UserStories.Count > 0)
            {
                return Result.Fail($"Cannot remove project with ID '{request.ProjectId}' because it has associated user stories. Set cascadeDelete to true to remove these user stories and their tasks as well.");
            }

            // If cascading, we need to handle the entire hierarchy
            if (request.CascadeDelete)
            {
                // In most databases, cascading deletes are more efficiently handled by the database itself
                // Since we've defined cascading deletes in our model using OnDelete(DeleteBehavior.Cascade),
                // we don't need to explicitly delete the child entities.
                // The call to Remove(project) will cascade to related entities automatically.
                
                // Note: If for some reason your database doesn't support cascading deletes,
                // or you're handling a situation where cascades aren't defined in the model,
                // you would need to manually delete the related entities in the correct order
                // (tasks first, then user stories, then the project), but make sure to avoid
                // tracking conflicts.
            }

            // Finally, remove the project
            context.Projects.Remove(project);
            await context.SaveChangesAsync(cancellationToken);
            
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to remove project: {ex.Message}");
        }
    }
}