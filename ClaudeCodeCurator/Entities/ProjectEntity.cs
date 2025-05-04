namespace ClaudeCodeCurator.Entities;

using System.ComponentModel.DataAnnotations;

public class ProjectEntity
{
    // Database will generate this value
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    // Navigation property - collection of user stories
    public virtual ICollection<UserStoryEntity> UserStories { get; set; } = new List<UserStoryEntity>();
}