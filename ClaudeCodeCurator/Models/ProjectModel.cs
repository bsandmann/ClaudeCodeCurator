namespace ClaudeCodeCurator.Models;

public class ProjectModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Navigation properties
    public List<UserStoryModel> UserStories { get; set; } = new();
}