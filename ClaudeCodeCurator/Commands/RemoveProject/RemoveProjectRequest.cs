namespace ClaudeCodeCurator.Commands.RemoveProject;

using FluentResults;
using MediatR;

public class RemoveProjectRequest : IRequest<Result<bool>>
{
    public Guid ProjectId { get; }

    /// <summary>
    /// If true, remove all associated user stories and tasks.
    /// If false and there are associated user stories, the operation will fail.
    /// </summary>
    public bool CascadeDelete { get; }

    public RemoveProjectRequest(Guid projectId, bool cascadeDelete = true)
    {
        ProjectId = projectId;
        CascadeDelete = cascadeDelete;
    }
}