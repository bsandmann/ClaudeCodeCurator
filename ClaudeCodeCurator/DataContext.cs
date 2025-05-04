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

    // DbSet property
    public DbSet<ProjectEntity> Projects { get; set; } = null!;


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
    }
}