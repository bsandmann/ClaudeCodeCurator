namespace ClaudeCodeCurator.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProjectTaskOrderEntity
{
    // Composite primary key through fluent API
    public Guid ProjectId { get; set; }
    
    public Guid TaskId { get; set; }
    
    // Position in the ordered list (unique per project)
    public int Position { get; set; }
    
    // Navigation properties
    public virtual ProjectEntity Project { get; set; } = null!;
    
    public virtual TaskEntity Task { get; set; } = null!;
}