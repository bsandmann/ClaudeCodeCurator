namespace ClaudeCodeCurator.Commands.GetUserStoryById;

using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;

public class GetUserStoryByIdRequest : IRequest<Result<UserStoryModel>>
{
    public Guid UserStoryId { get; }
    
    public bool ExcludeTasks { get; }

    public GetUserStoryByIdRequest(Guid userStoryId, bool excludeTasks = false)
    {
        UserStoryId = userStoryId;
        ExcludeTasks = excludeTasks;
    }
}