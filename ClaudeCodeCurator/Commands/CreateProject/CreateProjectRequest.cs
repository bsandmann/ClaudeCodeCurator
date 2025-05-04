namespace ClaudeCodeCurator.Commands.CreateProject;

using System.ComponentModel.DataAnnotations;
using FluentResults;
using MediatR;

public class CreateProjectRequest : IRequest<Result<Guid>>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; }

    public CreateProjectRequest(string name)
    {
        Name = name;
    }
}