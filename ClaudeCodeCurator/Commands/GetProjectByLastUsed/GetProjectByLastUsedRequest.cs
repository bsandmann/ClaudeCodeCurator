namespace ClaudeCodeCurator.Commands.GetProjectByLastUsed;

using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;

public class GetProjectByLastUsedRequest : IRequest<Result<ProjectModel>>
{
    public GetProjectByLastUsedRequest()
    {
        // No parameters needed for this request
    }
}