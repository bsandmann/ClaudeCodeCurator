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
    
    public bool ReferenceUserStory { get; }
    
    public bool PromptAppendThink { get; }
    public bool PromptAppendThinkHard { get; }
    public bool PromptAppendDoNotChange { get; }
    public bool UsePrimePrompt { get; }
    public bool UseVerifyPrompt { get; }

    public CreateTaskRequest(
        string name, 
        string promptBody, 
        Guid userStoryId, 
        TaskType type = TaskType.Task,
        bool referenceUserStory = false,
        bool promptAppendThink = false,
        bool promptAppendThinkHard = false,
        bool promptAppendDoNotChange = false,
        bool usePrimePrompt = false,
        bool useVerifyPrompt = false)
    {
        Name = name;
        PromptBody = promptBody;
        UserStoryId = userStoryId;
        Type = type;
        ReferenceUserStory = referenceUserStory;
        PromptAppendThink = promptAppendThink;
        PromptAppendThinkHard = promptAppendThinkHard;
        PromptAppendDoNotChange = promptAppendDoNotChange;
        UsePrimePrompt = usePrimePrompt;
        UseVerifyPrompt = useVerifyPrompt;
    }
}