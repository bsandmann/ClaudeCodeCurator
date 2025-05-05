namespace ClaudeCodeCurator.Commands.SetUserTaskApproval;

using FluentResults;
using MediatR;

public class SetUserTaskApprovalRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool ApprovedByUser { get; }
    
    public SetUserTaskApprovalRequest(Guid taskId, bool approvedByUser)
    {
        TaskId = taskId;
        ApprovedByUser = approvedByUser;
    }
}