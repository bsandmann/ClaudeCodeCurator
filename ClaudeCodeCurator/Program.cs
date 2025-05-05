using System.Text.Json.Serialization;
using ClaudeCodeCurator;
using ClaudeCodeCurator.Common;
using ClaudeCodeCurator.Components;
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


app.Run();