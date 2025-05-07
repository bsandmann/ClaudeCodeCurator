namespace ClaudeCodeCurator.Commands.SetResetTask;

using FluentResults;
using MediatR;

public class SetResetTaskRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public Guid? ProjectId { get; }
    
    public SetResetTaskRequest(Guid taskId, Guid? projectId = null)
    {
        TaskId = taskId;
        ProjectId = projectId;
    }
}