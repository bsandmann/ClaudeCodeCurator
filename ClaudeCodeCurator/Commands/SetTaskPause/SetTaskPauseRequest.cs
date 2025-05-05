namespace ClaudeCodeCurator.Commands.SetTaskPause;

using FluentResults;
using MediatR;

public class SetTaskPauseRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool Paused { get; }
    
    public SetTaskPauseRequest(Guid taskId, bool paused)
    {
        TaskId = taskId;
        Paused = paused;
    }
}