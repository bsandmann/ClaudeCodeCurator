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
    
    // Reference to parent user story by ID only
    public Guid UserStoryId { get; set; }
}