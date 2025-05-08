namespace ClaudeCodeCurator.Commands.UpdateSettings;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class UpdateSettingsHandler : IRequestHandler<UpdateSettingsRequest, Result<bool>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<UpdateSettingsHandler> _logger;

    public UpdateSettingsHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<UpdateSettingsHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateSettingsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            // Get the existing settings (we should only have one row)
            var settings = await context.Settings.FirstOrDefaultAsync(cancellationToken);
            
            if (settings == null)
            {
                _logger.LogError("Settings not found. This should not happen as the application should ensure one Settings row exists.");
                return Result.Fail("Settings not found.");
            }

            bool changed = false;
            
            // Update only the provided values (ignore nulls)
            if (request.OpenAiApiKey != null && settings.OpenAiApiKey != request.OpenAiApiKey)
            {
                settings.OpenAiApiKey = request.OpenAiApiKey;
                changed = true;
            }
            
            if (request.GoogleAiApiKey != null && settings.GoogleAiApiKey != request.GoogleAiApiKey)
            {
                settings.GoogleAiApiKey = request.GoogleAiApiKey;
                changed = true;
            }
            
            if (request.AnthropicAiApiKey != null && settings.AnthropicAiApiKey != request.AnthropicAiApiKey)
            {
                settings.AnthropicAiApiKey = request.AnthropicAiApiKey;
                changed = true;
            }
            
            if (request.OpenAiModel != null && settings.OpenAiModel != request.OpenAiModel)
            {
                settings.OpenAiModel = request.OpenAiModel;
                changed = true;
            }
            
            if (request.GoogleAiModel != null && settings.GoogleAiModel != request.GoogleAiModel)
            {
                settings.GoogleAiModel = request.GoogleAiModel;
                changed = true;
            }
            
            if (request.AnthropicAiModel != null && settings.AnthropicAiModel != request.AnthropicAiModel)
            {
                settings.AnthropicAiModel = request.AnthropicAiModel;
                changed = true;
            }

            if (changed)
            {
                // Save changes only if something was modified
                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Settings updated successfully.");
                return Result.Ok(true);
            }
            
            _logger.LogInformation("No changes were made to settings.");
            return Result.Ok(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating settings.");
            return Result.Fail(new Error("Failed to update settings.").CausedBy(ex));
        }
    }
}