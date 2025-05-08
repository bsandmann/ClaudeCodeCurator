namespace ClaudeCodeCurator.Tests;

using System;
using ClaudeCodeCurator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class TransactionalTestDatabaseFixture
{

    public const string ConnectionString = @"Host=10.10.20.103; Database=claudecodecuratortests; Username=postgres; Password=postgres; Include Error Detail=true";

    public DataContext CreateContext()
    {
        // var connectionString = System.Environment.GetEnvironmentVariable("");
        return new DataContext(
            new DbContextOptionsBuilder<DataContext>()
                .UseNpgsql(ConnectionString)
                .EnableSensitiveDataLogging(true)
                .LogTo(Console.WriteLine, LogLevel.Information)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options);
    }

    public TransactionalTestDatabaseFixture()
    {
        using var context = CreateContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Ensure Settings entity exists
        var settings = context.Settings.FirstOrDefault();
        if (settings == null)
        {
            context.Settings.Add(new Entities.SettingsEntity
            {
                OpenAiApiKey = string.Empty,
                GoogleAiApiKey = string.Empty,
                AnthropicAiApiKey = string.Empty,
                OpenAiModel = string.Empty,
                GoogleAiModel = string.Empty,
                AnthropicAiModel = string.Empty
            });
            context.SaveChanges();
        }
        
        Cleanup();
    }

    public void Cleanup()
    {
        using var context = CreateContext();
        
        // Clear all tables
        context.Tasks.RemoveRange(context.Tasks);
        context.UserStories.RemoveRange(context.UserStories);
        context.Projects.RemoveRange(context.Projects);
        
        // Reset Settings to a default state
        var settings = context.Settings.FirstOrDefault();
        if (settings != null)
        {
            settings.OpenAiApiKey = string.Empty;
            settings.GoogleAiApiKey = string.Empty;
            settings.AnthropicAiApiKey = string.Empty;
            settings.OpenAiModel = string.Empty;
            settings.GoogleAiModel = string.Empty;
            settings.AnthropicAiModel = string.Empty;
        }
        
        context.SaveChanges();
    }
}