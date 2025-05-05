namespace ClaudeCodeCurator.Commands.SetUserTaskApproval;

using FluentResults;
using MediatR;

public class SetUserTaskApprovalRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool Approve { get; }
    
    public SetUserTaskApprovalRequest(Guid taskId, bool approve)
    {
        TaskId = taskId;
        Approve = approve;
    }
}