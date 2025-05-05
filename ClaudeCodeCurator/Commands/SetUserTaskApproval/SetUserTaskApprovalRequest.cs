namespace ClaudeCodeCurator.Commands.SetUserTaskApproval;

using FluentResults;
using MediatR;

public class SetUserTaskApprovalRequest : IRequest<Result<bool>>
{
    public Guid TaskId { get; }
    public bool ApprovedByUser { get; }
    public Guid? ProjectId { get; }
    
    public SetUserTaskApprovalRequest(Guid taskId, bool approvedByUser, Guid? projectId = null)
    {
        TaskId = taskId;
        ApprovedByUser = approvedByUser;
        ProjectId = projectId;
    }
}