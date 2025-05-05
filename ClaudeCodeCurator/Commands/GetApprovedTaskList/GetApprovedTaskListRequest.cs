namespace ClaudeCodeCurator.Commands.GetApprovedTaskList;

using FluentResults;
using MediatR;
using ClaudeCodeCurator.Models;

public class GetApprovedTaskListRequest : IRequest<Result<List<TaskModel>>>
{
    public Guid ProjectId { get; }

    public GetApprovedTaskListRequest(Guid projectId)
    {
        ProjectId = projectId;
    }
}