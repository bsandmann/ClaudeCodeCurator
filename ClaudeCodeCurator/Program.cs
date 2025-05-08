using System.IO.Pipes;
using System.Text.Json.Serialization;
using ClaudeCodeCurator;
using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.GetProjectByLastUsed;
using ClaudeCodeCurator.Commands.GetProjectList;
using ClaudeCodeCurator.Common;
using ClaudeCodeCurator.Components;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.McpServer;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModelContextProtocol;

var builder = WebApplication.CreateBuilder(args);

// Configure the app to use HTTP by default
builder.WebHost.UseUrls("http://0.0.0.0:3330");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddRazorComponents(o => o.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents();
    
// Register services as singletons to maintain state across the application
builder.Services.AddSingleton<EditorState>();
builder.Services.AddSingleton<HumanizedTimeService>();
builder.Services.AddHttpContextAccessor();

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();

// Configure data source providers based on configuration
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
builder.Services.AddDbContext<DataContext>(options =>
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        // .EnableSensitiveDataLogging(true)
        .UseNpgsql(appSettings.ConnectionString)
        .ConfigureWarnings(p => p.Ignore(RelationalEventId.PendingModelChangesWarning))
        .UseNpgsql(p =>
        {
            p.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorCodesToAdd: null
            );
        }));
builder.Services.AddLogging(p =>
    p.AddConsole()
);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<CccTool>();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseStaticFiles();
app.UseRouting();

// app.MapControllers();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapMcp("/mcp");

// Apply migrations and initialize the database
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();
    
    // Apply pending migrations
    try
    {
        logger.LogInformation("Applying database migrations...");
        await dataContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
        
        // Ensure there's exactly one Settings row
        logger.LogInformation("Checking Settings table...");
        var settingsCount = await dataContext.Settings.CountAsync();
        
        if (settingsCount == 0)
        {
            // No settings row exists, create one
            logger.LogInformation("No Settings row found. Creating default Settings entry...");
            var defaultSettings = new ClaudeCodeCurator.Entities.SettingsEntity
            {
                OpenAiApiKey = string.Empty,
                GoogleAiApiKey = string.Empty,
                AnthropicAiApiKey = string.Empty,
                OpenAiModel = string.Empty,
                GoogleAiModel = string.Empty,
                AnthropicAiModel = string.Empty
            };
            
            // Create a new scope with a tracking context for adding entities
            using var settingsScope = app.Services.CreateScope();
            var settingsContext = settingsScope.ServiceProvider.GetRequiredService<DataContext>();
            
            settingsContext.Settings.Add(defaultSettings);
            await settingsContext.SaveChangesAsync();
            logger.LogInformation("Default Settings entry created successfully");
        }
        else if (settingsCount > 1)
        {
            // Multiple settings rows exist, keep only the first one
            logger.LogWarning($"Found {settingsCount} Settings rows. Removing extras to maintain only one row...");
            
            // Create a new scope with a tracking context for removing entities
            using var settingsScope = app.Services.CreateScope();
            var settingsContext = settingsScope.ServiceProvider.GetRequiredService<DataContext>();
            
            var allSettings = await settingsContext.Settings.ToListAsync();
            var firstSettings = allSettings.First();
            
            // Remove all settings rows except the first one
            settingsContext.Settings.RemoveRange(allSettings.Skip(1));
            await settingsContext.SaveChangesAsync();
            logger.LogInformation("Extra Settings rows removed. Now only one Settings row exists.");
        }
        else
        {
            logger.LogInformation("Verified: Exactly one Settings row exists.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw; // Re-throw to prevent app from starting with inconsistent database state
    }
    
    // Try to get the most recently used project first
    var getByLastUsedResult = await mediator.Send(new GetProjectByLastUsedRequest());
    
    if (!getByLastUsedResult.IsSuccess)
    {
        // No recently used project found, fall back to listing all projects
        logger.LogInformation("No recently used project found. Checking if any projects exist...");
        
        var projectListResult = await mediator.Send(new GetProjectListRequest());
        
        if (projectListResult.IsSuccess && projectListResult.Value.Count == 0)
        {
            // No projects exist, create a default project
            logger.LogInformation("No projects found. Creating default project...");
            var defaultProjectName = "My first project";
            var createProjectResult = await mediator.Send(new CreateProjectRequest(defaultProjectName));
            
            if (createProjectResult.IsSuccess)
            {
                logger.LogInformation($"Default project '{defaultProjectName}' created successfully");
            }
            else
            {
                logger.LogError($"Failed to create default project: {string.Join(", ", createProjectResult.Errors.Select(e => e.Message))}");
            }
        }
        else
        {
            logger.LogInformation($"Found {projectListResult.Value.Count} existing projects");
        }
    }
    else
    {
        logger.LogInformation($"Retrieved last used project: {getByLastUsedResult.Value.Name}");
    }
}

app.Run();