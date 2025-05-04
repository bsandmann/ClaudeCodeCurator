namespace ClaudeCodeCurator.Commands.RemoveUserStory;

using FluentResults;
using MediatR;

public class RemoveUserStoryRequest : IRequest<Result<bool>>
{
    public Guid UserStoryId { get; }

    /// <summary>
    /// If true, remove all associated tasks before removing the user story.
    /// If false and there are associated tasks, the operation will fail.
    /// </summary>
    public bool CascadeTasks { get; }

    public RemoveUserStoryRequest(Guid userStoryId, bool cascadeTasks = true)
    {
        UserStoryId = userStoryId;
        CascadeTasks = cascadeTasks;
    }
}