namespace ClaudeCodeCurator.Commands.SetAiTaskRequestState;

using FluentResults;
using MediatR;

public class SetAiTaskRequestStateRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool RequestedByAi { get; }
    
    public SetAiTaskRequestStateRequest(Guid taskId, bool requestedByAi)
    {
        TaskId = taskId;
        RequestedByAi = requestedByAi;
    }
}