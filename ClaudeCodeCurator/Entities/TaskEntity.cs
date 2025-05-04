namespace ClaudeCodeCurator.Entities;

using System.ComponentModel.DataAnnotations;

public class TaskEntity
{
    // Database will generate this value
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string PromptBody { get; set; } = string.Empty;
    
    // Task type with default value
    public TaskType Type { get; set; } = TaskType.Task;
    
    // Foreign key for UserStory
    public Guid UserStoryId { get; set; }
    
    // Navigation property
    public UserStoryEntity UserStory { get; set; } = null!;
}