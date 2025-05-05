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

            // Query for approved tasks by starting with Tasks that are approved
            // then joining with ProjectTaskOrders for the specific project
            // Order by Position in ProjectTaskOrders
            var approvedTasks = await context.Tasks
                .AsNoTracking()
                .Where(task => task.ApprovedByUserUtc != null) // Ensure task is approved
                .Join(
                    context.ProjectTaskOrders.Where(pto => pto.ProjectId == request.ProjectId),
                    task => task.Id,
                    pto => pto.TaskId,
                    (task, pto) => new { Task = task, TaskOrder = pto }
                )
                .OrderBy(x => x.TaskOrder.Position)
                .Select(x => new TaskModel
                {
                    Id = x.Task.Id,
                    Name = x.Task.Name,
                    PromptBody = x.Task.PromptBody,
                    TaskNumber = x.Task.TaskNumber,
                    Type = x.Task.Type,
                    UserStoryId = x.Task.UserStoryId,
                    ApprovedByUserUtc = x.Task.ApprovedByUserUtc,
                    RequestedByAiUtc = x.Task.RequestedByAiUtc,
                    FinishedByAiUtc = x.Task.FinishedByAiUtc,
                    CreatedOrUpdatedUtc = x.Task.CreatedOrUpdatedUtc,
                    Paused = x.Task.Paused
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