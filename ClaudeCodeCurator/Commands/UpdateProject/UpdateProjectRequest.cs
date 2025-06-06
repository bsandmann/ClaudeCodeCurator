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
    
    public string? PrimePrompt { get; }
    
    public string? VerifyPrompt { get; }

    public UpdateProjectRequest(Guid projectId, string name, string? primePrompt = null, string? verifyPrompt = null)
    {
        ProjectId = projectId;
        Name = name;
        PrimePrompt = primePrompt;
        VerifyPrompt = verifyPrompt;
    }
}