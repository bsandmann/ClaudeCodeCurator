namespace ClaudeCodeCurator.Models;

public class ProjectModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Counters for auto-numbering
    public int UserStoryNumberCounter { get; set; }
    public int TaskNumberCounter { get; set; }
    
    // Creation/update timestamp
    public DateTime CreatedOrUpdatedUtc { get; set; }
    
    // Navigation properties
    public List<UserStoryModel> UserStories { get; set; } = new();
}