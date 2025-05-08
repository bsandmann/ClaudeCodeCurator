using ClaudeCodeCurator.Commands.GetSettings;
using ClaudeCodeCurator.Commands.UpdateSettings;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task Update_Settings_All_Fields_Succeeds()
    {
        // Arrange
        var openAiApiKey = "openai-key-123";
        var googleAiApiKey = "google-key-123";
        var anthropicAiApiKey = "anthropic-key-123";
        var openAiModel = "gpt-4";
        var googleAiModel = "gemini-pro";
        var anthropicAiModel = "claude-3-opus";
        
        var updateSettingsRequest = new UpdateSettingsRequest(
            openAiApiKey: openAiApiKey,
            googleAiApiKey: googleAiApiKey,
            anthropicAiApiKey: anthropicAiApiKey,
            openAiModel: openAiModel,
            googleAiModel: googleAiModel,
            anthropicAiModel: anthropicAiModel
        );
        
        // Act
        var updateResult = await _updateSettingsHandler.Handle(updateSettingsRequest, CancellationToken.None);
        
        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Should().BeTrue(); // Should return true indicating a change was made
        
        // Verify the settings were actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var settings = await context.Settings.AsNoTracking().FirstOrDefaultAsync();
            settings.Should().NotBeNull();
            settings!.OpenAiApiKey.Should().Be(openAiApiKey);
            settings.GoogleAiApiKey.Should().Be(googleAiApiKey);
            settings.AnthropicAiApiKey.Should().Be(anthropicAiApiKey);
            settings.OpenAiModel.Should().Be(openAiModel);
            settings.GoogleAiModel.Should().Be(googleAiModel);
            settings.AnthropicAiModel.Should().Be(anthropicAiModel);
        }
    }
    
    [Fact]
    public async Task Update_Settings_Partial_Fields_Succeeds()
    {
        // Arrange - First update all fields to known values
        var initialOpenAiApiKey = "initial-openai-key";
        var initialGoogleAiApiKey = "initial-google-key";
        var initialAnthropicAiApiKey = "initial-anthropic-key";
        var initialOpenAiModel = "initial-openai-model";
        var initialGoogleAiModel = "initial-google-model";
        var initialAnthropicAiModel = "initial-anthropic-model";
        
        await _updateSettingsHandler.Handle(new UpdateSettingsRequest(
            openAiApiKey: initialOpenAiApiKey,
            googleAiApiKey: initialGoogleAiApiKey,
            anthropicAiApiKey: initialAnthropicAiApiKey,
            openAiModel: initialOpenAiModel,
            googleAiModel: initialGoogleAiModel,
            anthropicAiModel: initialAnthropicAiModel
        ), CancellationToken.None);
        
        // Now update only some fields
        var newOpenAiApiKey = "new-openai-key";
        var newGoogleAiModel = "new-google-model";
        
        var updateSettingsRequest = new UpdateSettingsRequest(
            openAiApiKey: newOpenAiApiKey,
            googleAiModel: newGoogleAiModel
        );
        
        // Act
        var updateResult = await _updateSettingsHandler.Handle(updateSettingsRequest, CancellationToken.None);
        
        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Should().BeTrue(); // Should return true indicating a change was made
        
        // Verify the settings were properly updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var settings = await context.Settings.AsNoTracking().FirstOrDefaultAsync();
            settings.Should().NotBeNull();
            
            // Updated fields
            settings!.OpenAiApiKey.Should().Be(newOpenAiApiKey);
            settings.GoogleAiModel.Should().Be(newGoogleAiModel);
            
            // Unchanged fields
            settings.GoogleAiApiKey.Should().Be(initialGoogleAiApiKey);
            settings.AnthropicAiApiKey.Should().Be(initialAnthropicAiApiKey);
            settings.OpenAiModel.Should().Be(initialOpenAiModel);
            settings.AnthropicAiModel.Should().Be(initialAnthropicAiModel);
        }
    }
    
    [Fact]
    public async Task Update_Settings_With_Same_Values_Returns_NoChange()
    {
        // Arrange - First update all fields to known values
        var openAiApiKey = "test-openai-key";
        var googleAiApiKey = "test-google-key";
        var anthropicAiApiKey = "test-anthropic-key";
        var openAiModel = "test-openai-model";
        var googleAiModel = "test-google-model";
        var anthropicAiModel = "test-anthropic-model";
        
        await _updateSettingsHandler.Handle(new UpdateSettingsRequest(
            openAiApiKey: openAiApiKey,
            googleAiApiKey: googleAiApiKey,
            anthropicAiApiKey: anthropicAiApiKey,
            openAiModel: openAiModel,
            googleAiModel: googleAiModel,
            anthropicAiModel: anthropicAiModel
        ), CancellationToken.None);
        
        // Fetch the current values from the database to make sure we're comparing against what's actually there
        string currentOpenAiApiKey;
        string currentGoogleAiApiKey;
        string currentAnthropicAiApiKey;
        string currentOpenAiModel;
        string currentGoogleAiModel;
        string currentAnthropicAiModel;
        
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var settings = await context.Settings.AsNoTracking().FirstOrDefaultAsync();
            if (settings == null)
            {
                Assert.Fail("Settings not found in database");
                return;
            }
            
            currentOpenAiApiKey = settings.OpenAiApiKey;
            currentGoogleAiApiKey = settings.GoogleAiApiKey;
            currentAnthropicAiApiKey = settings.AnthropicAiApiKey;
            currentOpenAiModel = settings.OpenAiModel;
            currentGoogleAiModel = settings.GoogleAiModel;
            currentAnthropicAiModel = settings.AnthropicAiModel;
        }
        
        // Now update with the same values
        var updateSettingsRequest = new UpdateSettingsRequest(
            openAiApiKey: currentOpenAiApiKey,
            googleAiApiKey: currentGoogleAiApiKey,
            anthropicAiApiKey: currentAnthropicAiApiKey,
            openAiModel: currentOpenAiModel,
            googleAiModel: currentGoogleAiModel,
            anthropicAiModel: currentAnthropicAiModel
        );
        
        // Act
        var updateResult = await _updateSettingsHandler.Handle(updateSettingsRequest, CancellationToken.None);
        
        // Assert
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value.Should().BeFalse(); // Should return false indicating no change was made
    }
}