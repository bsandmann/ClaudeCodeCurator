namespace ClaudeCodeCurator.Commands.CreateTask;

using System.ComponentModel.DataAnnotations;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;

public class CreateTaskRequest : IRequest<Result<Guid>>
{
    [Required]
    [MaxLength(500)]
    public string Name { get; }
    
    [Required]
    public string PromptBody { get; }
    
    [Required]
    public Guid UserStoryId { get; }
    
    public TaskType Type { get; }

    public CreateTaskRequest(
        string name, 
        string promptBody, 
        Guid userStoryId, 
        TaskType type = TaskType.Task)
    {
        Name = name;
        PromptBody = promptBody;
        UserStoryId = userStoryId;
        Type = type;
    }
}