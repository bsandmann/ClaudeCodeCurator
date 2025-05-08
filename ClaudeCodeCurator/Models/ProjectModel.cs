namespace ClaudeCodeCurator.Models;

public class ProjectModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PrimePrompt { get; set; }
    public string? VerifyPrompt { get; set; }
    
    // Counters for auto-numbering
    public int UserStoryNumberCounter { get; set; }
    public int TaskNumberCounter { get; set; }
    
    // Creation/update timestamp
    public DateTime CreatedOrUpdatedUtc { get; set; }
    
    // Navigation properties
    public List<UserStoryModel> UserStories { get; set; } = new();
}