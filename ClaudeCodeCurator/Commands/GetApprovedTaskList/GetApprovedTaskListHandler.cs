namespace ClaudeCodeCurator.Commands.GetApprovedTaskList;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class GetApprovedTaskListHandler : IRequestHandler<GetApprovedTaskListRequest, Result<List<TaskModel>>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GetApprovedTaskListHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<List<TaskModel>>> Handle(GetApprovedTaskListRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Verify project exists
            var projectExists = await context.Projects
                .AsNoTracking()
                .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (!projectExists)
            {
                return Result.Fail($"Project with ID '{request.ProjectId}' not found");
            }

            // Query for approved tasks using navigation properties
            // Start with ProjectTaskOrders for the specific project
            // Then navigate to Tasks that are approved
            var approvedTasks = await context.ProjectTaskOrders
                .AsNoTracking()
                .Where(pto => pto.ProjectId == request.ProjectId)
                .Include(pto => pto.Task)
                    .ThenInclude(task => task.UserStory)
                .Where(pto => pto.Task.ApprovedByUserUtc != null) // Ensure task is approved
                .OrderBy(pto => pto.Position)
                .Select(pto => new TaskModel
                {
                    Id = pto.Task.Id,
                    Name = pto.Task.Name,
                    PromptBody = pto.Task.PromptBody,
                    TaskNumber = pto.Task.TaskNumber,
                    Type = pto.Task.Type,
                    ReferenceUserStory = pto.Task.ReferenceUserStory,
                    UserStoryId = pto.Task.UserStoryId,
                    UserStoryNumber = pto.Task.UserStory.UserStoryNumber,
                    ApprovedByUserUtc = pto.Task.ApprovedByUserUtc,
                    RequestedByAiUtc = pto.Task.RequestedByAiUtc,
                    FinishedByAiUtc = pto.Task.FinishedByAiUtc,
                    CreatedOrUpdatedUtc = pto.Task.CreatedOrUpdatedUtc,
                    Paused = pto.Task.Paused
                })
                .ToListAsync(cancellationToken);

            return Result.Ok(approvedTasks);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve approved tasks: {ex.Message}");
        }
    }
}