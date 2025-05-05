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
    
    // Auto-incremented number within project
    public int TaskNumber { get; set; }
    
    // Task type with default value
    public TaskType Type { get; set; } = TaskType.Task;
    
    // Date properties for tracking task lifecycle
    public DateTime? ApprovedByUserUtc { get; set; }
    public DateTime? RequestedByAiUtc { get; set; }
    public DateTime? FinishedByAiUtc { get; set; }
    
    // Task can be paused to temporarily exclude it from processing
    public bool Paused { get; set; } = false;
    
    // Creation/update timestamp
    public DateTime CreatedOrUpdatedUtc { get; set; }
    
    // Foreign key for UserStory
    public Guid UserStoryId { get; set; }
    
    // Navigation property
    public UserStoryEntity UserStory { get; set; } = null!;
    
    // Navigation property - project ordering relationships
    public virtual ICollection<ProjectTaskOrderEntity> ProjectOrders { get; set; } = new List<ProjectTaskOrderEntity>();
}