namespace ClaudeCodeCurator.Commands.GetTaskById;

using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;

public class GetTaskByIdRequest : IRequest<Result<TaskModel>>
{
    public Guid TaskId { get; }

    public GetTaskByIdRequest(Guid taskId)
    {
        TaskId = taskId;
    }
}