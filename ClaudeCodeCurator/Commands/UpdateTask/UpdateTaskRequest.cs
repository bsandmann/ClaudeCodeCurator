namespace ClaudeCodeCurator.Commands.UpdateTask;

using System.ComponentModel.DataAnnotations;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;

public class UpdateTaskRequest : IRequest<Result<bool>>
{
    [Required]
    public Guid TaskId { get; }
    
    [Required]
    [MaxLength(500)]
    public string Name { get; }
    
    [Required]
    public string PromptBody { get; }
    
    public TaskType Type { get; }
    
    public string ReferenceUserStory { get; }

    public UpdateTaskRequest(
        Guid taskId, 
        string name, 
        string promptBody, 
        TaskType type,
        string referenceUserStory = "")
    {
        TaskId = taskId;
        Name = name;
        PromptBody = promptBody;
        Type = type;
        ReferenceUserStory = referenceUserStory;
    }
}