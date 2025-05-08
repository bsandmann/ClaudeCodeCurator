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
    
    public bool ReferenceUserStory { get; }
    
    public bool PromptAppendThink { get; }
    public bool PromptAppendThinkHard { get; }
    public bool PromptAppendDoNotChange { get; }
    public bool UsePrimePrompt { get; }
    public bool UseVerifyPrompt { get; }

    public UpdateTaskRequest(
        Guid taskId, 
        string name, 
        string promptBody, 
        TaskType type,
        bool referenceUserStory = false,
        bool promptAppendThink = false,
        bool promptAppendThinkHard = false,
        bool promptAppendDoNotChange = false,
        bool usePrimePrompt = false,
        bool useVerifyPrompt = false)
    {
        TaskId = taskId;
        Name = name;
        PromptBody = promptBody;
        Type = type;
        ReferenceUserStory = referenceUserStory;
        PromptAppendThink = promptAppendThink;
        PromptAppendThinkHard = promptAppendThinkHard;
        PromptAppendDoNotChange = promptAppendDoNotChange;
        UsePrimePrompt = usePrimePrompt;
        UseVerifyPrompt = useVerifyPrompt;
    }
}