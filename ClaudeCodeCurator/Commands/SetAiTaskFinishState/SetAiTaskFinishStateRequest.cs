namespace ClaudeCodeCurator.Commands.SetAiTaskFinishState;

using FluentResults;
using MediatR;

public class SetAiTaskFinishStateRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool Finish { get; }
    
    public SetAiTaskFinishStateRequest(Guid taskId, bool finish)
    {
        TaskId = taskId;
        Finish = finish;
    }
}