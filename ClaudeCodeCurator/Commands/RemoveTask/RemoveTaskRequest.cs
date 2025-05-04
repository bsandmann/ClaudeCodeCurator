namespace ClaudeCodeCurator.Commands.RemoveTask;

using FluentResults;
using MediatR;

public class RemoveTaskRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }

    public RemoveTaskRequest(Guid taskId)
    {
        TaskId = taskId;
    }
}