using ClaudeCodeCurator.Commands.GetSettings;
using ClaudeCodeCurator.Commands.UpdateSettings;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task Get_Settings_Returns_Default_Settings()
    {
        // Arrange - Reset settings to default state
        // Use the UpdateSettings handler to reset values
        var updateResult = await _updateSettingsHandler.Handle(new UpdateSettingsRequest(
            openAiApiKey: string.Empty,
            googleAiApiKey: string.Empty,
            anthropicAiApiKey: string.Empty,
            openAiModel: string.Empty,
            googleAiModel: string.Empty,
            anthropicAiModel: string.Empty
        ), CancellationToken.None);
        
        updateResult.IsSuccess.Should().BeTrue();
        
        // Act
        var result = await _getSettingsHandler.Handle(new GetSettingsRequest(), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.OpenAiApiKey.Should().BeEmpty();
        result.Value.GoogleAiApiKey.Should().BeEmpty();
        result.Value.AnthropicAiApiKey.Should().BeEmpty();
        result.Value.OpenAiModel.Should().BeEmpty();
        result.Value.GoogleAiModel.Should().BeEmpty();
        result.Value.AnthropicAiModel.Should().BeEmpty();
    }
    
    [Fact]
    public async Task Get_Settings_Returns_Updated_Settings()
    {
        // Arrange - Update settings to new values
        var openAiApiKey = "get-test-openai-key";
        var googleAiApiKey = "get-test-google-key";
        var anthropicAiApiKey = "get-test-anthropic-key";
        var openAiModel = "get-test-openai-model";
        var googleAiModel = "get-test-google-model";
        var anthropicAiModel = "get-test-anthropic-model";
        
        await _updateSettingsHandler.Handle(new UpdateSettingsRequest(
            openAiApiKey: openAiApiKey,
            googleAiApiKey: googleAiApiKey,
            anthropicAiApiKey: anthropicAiApiKey,
            openAiModel: openAiModel,
            googleAiModel: googleAiModel,
            anthropicAiModel: anthropicAiModel
        ), CancellationToken.None);
        
        // Act
        var result = await _getSettingsHandler.Handle(new GetSettingsRequest(), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.OpenAiApiKey.Should().Be(openAiApiKey);
        result.Value.GoogleAiApiKey.Should().Be(googleAiApiKey);
        result.Value.AnthropicAiApiKey.Should().Be(anthropicAiApiKey);
        result.Value.OpenAiModel.Should().Be(openAiModel);
        result.Value.GoogleAiModel.Should().Be(googleAiModel);
        result.Value.AnthropicAiModel.Should().Be(anthropicAiModel);
    }
    
    [Fact]
    public async Task Get_Settings_Correctly_Returns_Model_From_Entity()
    {
        // Arrange - Update settings with the UpdateSettingsHandler first
        var openAiApiKey = "direct-update-openai-key";
        var googleAiApiKey = "direct-update-google-key";
        var anthropicAiApiKey = "direct-update-anthropic-key";
        var openAiModel = "direct-update-openai-model";
        var googleAiModel = "direct-update-google-model";
        var anthropicAiModel = "direct-update-anthropic-model";
        
        // Use the handler to update the settings
        var updateResult = await _updateSettingsHandler.Handle(new UpdateSettingsRequest(
            openAiApiKey: openAiApiKey,
            googleAiApiKey: googleAiApiKey,
            anthropicAiApiKey: anthropicAiApiKey,
            openAiModel: openAiModel,
            googleAiModel: googleAiModel,
            anthropicAiModel: anthropicAiModel
        ), CancellationToken.None);
        
        updateResult.IsSuccess.Should().BeTrue();
        
        // Act
        var result = await _getSettingsHandler.Handle(new GetSettingsRequest(), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.OpenAiApiKey.Should().Be(openAiApiKey);
        result.Value.GoogleAiApiKey.Should().Be(googleAiApiKey);
        result.Value.AnthropicAiApiKey.Should().Be(anthropicAiApiKey);
        result.Value.OpenAiModel.Should().Be(openAiModel);
        result.Value.GoogleAiModel.Should().Be(googleAiModel);
        result.Value.AnthropicAiModel.Should().Be(anthropicAiModel);
    }
}