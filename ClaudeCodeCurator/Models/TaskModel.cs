namespace ClaudeCodeCurator.Models;

using ClaudeCodeCurator.Entities;

public class TaskModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PromptBody { get; set; } = string.Empty;
    public TaskType Type { get; set; } = TaskType.Task;
    
    // Reference to parent user story by ID only
    public Guid UserStoryId { get; set; }
}