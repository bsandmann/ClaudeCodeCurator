namespace ClaudeCodeCurator.Commands.UpdateProject;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class UpdateProjectHandler : IRequestHandler<UpdateProjectRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UpdateProjectHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<bool>> Handle(UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Find the project by ID and ensure it's tracked by the context
            var project = await context.Projects
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (project == null)
            {
                return Result.Fail($"Project with ID '{request.ProjectId}' not found");
            }

            // Check if a different project with the same name already exists
            var existingProject = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.Name == request.Name && p.Id != request.ProjectId, 
                    cancellationToken);

            if (existingProject != null)
            {
                return Result.Fail($"A different project with name '{request.Name}' already exists");
            }

            // Check if name actually changed (optimization)
            if (project.Name == request.Name)
            {
                return Result.Ok(false); // No change needed
            }

            // Update the project name
            project.Name = request.Name;
            project.CreatedOrUpdatedUtc = DateTime.UtcNow;
            
            // No need to call context.Update(project) since the entity is tracked
            await context.SaveChangesAsync(cancellationToken);
            
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update project: {ex.Message}");
        }
    }
}