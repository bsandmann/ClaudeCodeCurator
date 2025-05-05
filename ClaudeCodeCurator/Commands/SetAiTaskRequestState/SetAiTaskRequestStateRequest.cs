namespace ClaudeCodeCurator.Commands.SetAiTaskRequestState;

using FluentResults;
using MediatR;

public class SetAiTaskRequestStateRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool RequestByAi { get; }
    
    public SetAiTaskRequestStateRequest(Guid taskId, bool requestByAi)
    {
        TaskId = taskId;
        RequestByAi = requestByAi;
    }
}