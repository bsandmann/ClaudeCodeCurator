namespace ClaudeCodeCurator.Commands.GetProjectList;

using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;

public class GetProjectListRequest : IRequest<Result<List<ProjectModel>>>
{
    public GetProjectListRequest()
    {
        // No parameters needed for this request
    }
}