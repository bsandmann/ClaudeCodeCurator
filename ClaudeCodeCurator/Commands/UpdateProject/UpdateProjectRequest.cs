namespace ClaudeCodeCurator.Commands.UpdateProject;

using System.ComponentModel.DataAnnotations;
using FluentResults;
using MediatR;

public class UpdateProjectRequest : IRequest<Result<bool>>
{
    [Required]
    public Guid ProjectId { get; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; }

    public UpdateProjectRequest(Guid projectId, string name)
    {
        ProjectId = projectId;
        Name = name;
    }
}