namespace ClaudeCodeCurator.Commands.UpdateUserStory;

using System.ComponentModel.DataAnnotations;
using FluentResults;
using MediatR;

public class UpdateUserStoryRequest : IRequest<Result<bool>>
{
    [Required]
    public Guid UserStoryId { get; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; }
    
    public string? Description { get; }

    public UpdateUserStoryRequest(Guid userStoryId, string name, string? description = null)
    {
        UserStoryId = userStoryId;
        Name = name;
        Description = description;
    }
}