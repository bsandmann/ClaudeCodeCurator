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
    
    // Navigation property
    public ProjectEntity Project { get; set; } = null!;
}