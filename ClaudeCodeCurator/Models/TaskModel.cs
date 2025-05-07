namespace ClaudeCodeCurator.Models;

using ClaudeCodeCurator.Entities;

public class TaskModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PromptBody { get; set; } = string.Empty;
    
    // Auto-incremented number within project
    public int TaskNumber { get; set; }
    
    public TaskType Type { get; set; } = TaskType.Task;
    
    // Date properties for tracking task lifecycle
    public DateTime? ApprovedByUserUtc { get; set; }
    public DateTime? RequestedByAiUtc { get; set; }
    public DateTime? FinishedByAiUtc { get; set; }
    
    // Task can be paused to temporarily exclude it from processing
    public bool Paused { get; set; } = false;
    
    // Creation/update timestamp
    public DateTime CreatedOrUpdatedUtc { get; set; }
    
    // Reference to parent user story by ID only
    public Guid UserStoryId { get; set; }
    
    // User story number for display purposes
    public int UserStoryNumber { get; set; }
}