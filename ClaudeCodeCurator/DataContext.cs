namespace ClaudeCodeCurator;

using ClaudeCodeCurator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    // Constructor that accepts DbContextOptions
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    // DbSet properties
    public DbSet<ProjectEntity> Projects { get; set; } = null!;
    public DbSet<UserStoryEntity> UserStories { get; set; } = null!;
    public DbSet<TaskEntity> Tasks { get; set; } = null!;
    public DbSet<ProjectTaskOrderEntity> ProjectTaskOrders { get; set; } = null!;
    public DbSet<SettingsEntity> Settings { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<UserStoryEntity>(entity =>
        {
            entity.ToTable("UserStories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired(false);
            
            // Define relationship with Project
            entity.HasOne(d => d.Project)
                .WithMany(p => p.UserStories)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PromptBody).IsRequired();
            
            // Configure TaskType as int in the database
            entity.Property(e => e.Type)
                .HasConversion<int>()
                .HasDefaultValue(TaskType.Task);
            
            // Define relationship with UserStory
            entity.HasOne(d => d.UserStory)
                .WithMany(us => us.Tasks)
                .HasForeignKey(d => d.UserStoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        
        modelBuilder.Entity<ProjectTaskOrderEntity>(entity =>
        {
            entity.ToTable("ProjectTaskOrders");
            
            // Configure composite primary key
            entity.HasKey(e => new { e.ProjectId, e.TaskId });
            
            // Create a unique constraint for project+position combination
            entity.HasIndex(e => new { e.ProjectId, e.Position }).IsUnique();
            
            // Define relationship with Project
            entity.HasOne(d => d.Project)
                .WithMany(p => p.OrderedTasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
                
            // Define relationship with Task
            entity.HasOne(d => d.Task)
                .WithMany(t => t.ProjectOrders)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        
        modelBuilder.Entity<SettingsEntity>(entity =>
        {
            entity.ToTable("Settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.OpenAiApiKey).HasMaxLength(200);
            entity.Property(e => e.GoogleAiApiKey).HasMaxLength(200);
            entity.Property(e => e.AnthropicAiApiKey).HasMaxLength(200);
            entity.Property(e => e.OpenAiModel).HasMaxLength(100);
            entity.Property(e => e.GoogleAiModel).HasMaxLength(100);
            entity.Property(e => e.AnthropicAiModel).HasMaxLength(100);
        });
    }
}