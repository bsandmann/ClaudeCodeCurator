namespace ClaudeCodeCurator.Commands.GetProjectById;

using System;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;

public class GetProjectByIdRequest : IRequest<Result<ProjectModel>>
{
    public Guid ProjectId { get; }
    
    public bool ExcludeUserStories { get; }
    
    public bool ExcludeTasks { get; }

    public GetProjectByIdRequest(Guid projectId, bool excludeUserStories = false, bool excludeTasks = false)
    {
        ProjectId = projectId;
        ExcludeUserStories = excludeUserStories;
        
        // If user stories are excluded, tasks must also be excluded
        ExcludeTasks = excludeUserStories || excludeTasks;
    }
}