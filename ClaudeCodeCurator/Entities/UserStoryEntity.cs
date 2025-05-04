namespace ClaudeCodeCurator.Entities;

using System.ComponentModel.DataAnnotations;

public class UserStoryEntity
{
    // Database will generate this value
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    // Foreign key for Project
    public Guid ProjectId { get; set; }
    
    // Navigation property - parent project
    public virtual ProjectEntity Project { get; set; } = null!;
    
    // Navigation property - collection of tasks
    public virtual ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}