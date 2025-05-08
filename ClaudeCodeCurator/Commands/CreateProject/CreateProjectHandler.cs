namespace ClaudeCodeCurator.Commands.CreateProject;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class CreateProjectHandler : IRequestHandler<CreateProjectRequest, Result<Guid>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateProjectHandler(
        IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<Guid>> Handle(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Check if a project with the same name already exists
            var existingProject = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

            if (existingProject != null)
            {
                return Result.Fail($"Project with name '{request.Name}' already exists");
            }

            // Create new project
            var project = new ProjectEntity
            {
                Name = request.Name,
                PrimePrompt = request.PrimePrompt,
                CreatedOrUpdatedUtc = DateTime.UtcNow
            };

            await context.Projects.AddAsync(project, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(project.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create project: {ex.Message}");
        }
    }
}