namespace ClaudeCodeCurator.Commands.CreateUserStory;

using System.ComponentModel.DataAnnotations;
using FluentResults;
using MediatR;

public class CreateUserStoryRequest : IRequest<Result<Guid>>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; }
    
    public string? Description { get; }
    
    [Required]
    public Guid ProjectId { get; }

    public CreateUserStoryRequest(string name, Guid projectId, string? description = null)
    {
        Name = name;
        ProjectId = projectId;
        Description = description;
    }
}