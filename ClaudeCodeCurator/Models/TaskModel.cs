namespace ClaudeCodeCurator.Models;

using ClaudeCodeCurator.Entities;

public class TaskModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PromptBody { get; set; } = string.Empty;
    public TaskType Type { get; set; } = TaskType.Task;
    
    public Guid UserStoryId { get; set; }
    
    // Navigation property
    public UserStoryModel? UserStory { get; set; }
}