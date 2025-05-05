namespace ClaudeCodeCurator.Entities;

using System.ComponentModel.DataAnnotations;

public class ProjectEntity
{
    // Database will generate this value
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public int UserStoryNumberCounter { get; set; }
    
    public int TaskNumberCounter { get; set; }
    
    // Creation/update timestamp
    public DateTime CreatedOrUpdatedUtc { get; set; }
    
    // Navigation property - collection of user stories
    public virtual ICollection<UserStoryEntity> UserStories { get; set; } = new List<UserStoryEntity>();
    
    // Navigation property - ordered tasks relationship
    public virtual ICollection<ProjectTaskOrderEntity> OrderedTasks { get; set; } = new List<ProjectTaskOrderEntity>();
}