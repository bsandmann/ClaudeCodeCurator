namespace ClaudeCodeCurator.Commands.CreateProject;

using System.ComponentModel.DataAnnotations;
using FluentResults;
using MediatR;

public class CreateProjectRequest : IRequest<Result<Guid>>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; }
    
    public string? PrimePrompt { get; }
    
    public string? VerifyPrompt { get; }

    public CreateProjectRequest(string name, string? primePrompt = null, string? verifyPrompt = null)
    {
        Name = name;
        PrimePrompt = primePrompt;
        VerifyPrompt = verifyPrompt;
    }
}