namespace ClaudeCodeCurator.Models;

using ClaudeCodeCurator.Entities;

public class SettingsModel
{
    public Guid Id { get; set; }
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string GoogleAiApiKey { get; set; } = string.Empty;
    public string AnthropicAiApiKey { get; set; } = string.Empty;
    public string OpenAiModel { get; set; } = string.Empty;
    public string GoogleAiModel { get; set; } = string.Empty;
    public string AnthropicAiModel { get; set; } = string.Empty;
    
    public static SettingsModel FromEntity(SettingsEntity entity)
    {
        return new SettingsModel
        {
            Id = entity.Id,
            OpenAiApiKey = entity.OpenAiApiKey,
            GoogleAiApiKey = entity.GoogleAiApiKey,
            AnthropicAiApiKey = entity.AnthropicAiApiKey,
            OpenAiModel = entity.OpenAiModel,
            GoogleAiModel = entity.GoogleAiModel,
            AnthropicAiModel = entity.AnthropicAiModel
        };
    }
}