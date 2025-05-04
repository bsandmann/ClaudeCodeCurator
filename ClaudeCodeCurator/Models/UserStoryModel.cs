namespace ClaudeCodeCurator.Models;

public class UserStoryModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public Guid ProjectId { get; set; }
    
    // Navigation properties
    public ProjectModel? Project { get; set; }
    public List<TaskModel> Tasks { get; set; } = new();
}