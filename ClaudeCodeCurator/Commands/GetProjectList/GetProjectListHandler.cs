namespace ClaudeCodeCurator.Commands.GetProjectList;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class GetProjectListHandler : IRequestHandler<GetProjectListRequest, Result<List<ProjectModel>>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GetProjectListHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<List<ProjectModel>>> Handle(GetProjectListRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Query all projects without including any relations
            var projectEntities = await context.Projects
                .AsNoTracking()
                .OrderBy(p => p.CreatedOrUpdatedUtc)
                .ToListAsync(cancellationToken);

            // Map project entities to models
            var projectModels = projectEntities.Select(p => new ProjectModel
            {
                Id = p.Id,
                Name = p.Name,
                UserStoryNumberCounter = p.UserStoryNumberCounter,
                TaskNumberCounter = p.TaskNumberCounter,
                CreatedOrUpdatedUtc = p.CreatedOrUpdatedUtc,
                UserStories = new List<UserStoryModel>() // Empty list since we're not including user stories
            }).ToList();

            return Result.Ok(projectModels);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve projects: {ex.Message}");
        }
    }
}