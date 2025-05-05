namespace ClaudeCodeCurator.Models;

public class UserStoryModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Auto-incremented number within project
    public int UserStoryNumber { get; set; }
    
    // Creation/update timestamp
    public DateTime CreatedOrUpdatedUtc { get; set; }
    
    // Reference to parent project by ID only
    public Guid ProjectId { get; set; }
    
    // Collection of child tasks
    public List<TaskModel> Tasks { get; set; } = new();
}