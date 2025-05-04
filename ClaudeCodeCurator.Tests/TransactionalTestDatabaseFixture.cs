namespace ClaudeCodeCurator.Tests;

using System;
using ClaudeCodeCurator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class TransactionalTestDatabaseFixture
{

    private const string ConnectionString = @"Host=10.10.20.103; Database=claudecodecuratortests; Username=postgres; Password=postgres; Include Error Detail=true";

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
        Cleanup();
    }

    public void Cleanup()
    {
        // using var context = CreateContext();
        // context.SaveChanges();
    }
}