using System.Text.Json.Serialization;
using ClaudeCodeCurator;
using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.GetProjectList;
using ClaudeCodeCurator.Common;
using ClaudeCodeCurator.Components;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddRazorComponents(o => o.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents();

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

var app = builder.Build();

app.UseForwardedHeaders();

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw; // Re-throw to prevent app from starting with inconsistent database state
    }
    
    // Get the list of existing projects
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
}

app.Run();