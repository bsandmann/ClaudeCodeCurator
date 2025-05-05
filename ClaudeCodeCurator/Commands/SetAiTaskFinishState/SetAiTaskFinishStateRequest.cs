namespace ClaudeCodeCurator.Commands.SetAiTaskFinishState;

using FluentResults;
using MediatR;

public class SetAiTaskFinishStateRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool FinishedByAi { get; }
    
    public SetAiTaskFinishStateRequest(Guid taskId, bool finishedByAi)
    {
        TaskId = taskId;
        FinishedByAi = finishedByAi;
    }
}