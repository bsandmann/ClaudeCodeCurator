namespace ClaudeCodeCurator.Commands.GetSettings;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class GetSettingsHandler : IRequestHandler<GetSettingsRequest, Result<SettingsModel>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GetSettingsHandler> _logger;

    public GetSettingsHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<GetSettingsHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<Result<SettingsModel>> Handle(GetSettingsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            // Get the existing settings (we should only have one row)
            var settings = await context.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
            
            if (settings == null)
            {
                _logger.LogError("Settings not found. This should not happen as the application should ensure one Settings row exists.");
                return Result.Fail("Settings not found.");
            }

            var settingsModel = SettingsModel.FromEntity(settings);
            return Result.Ok(settingsModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving settings.");
            return Result.Fail(new Error("Failed to retrieve settings.").CausedBy(ex));
        }
    }
}