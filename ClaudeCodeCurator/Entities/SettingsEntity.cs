namespace ClaudeCodeCurator.Entities;

using System.ComponentModel.DataAnnotations;

public class SettingsEntity
{
    // Database will generate this value
    public Guid Id { get; set; }
    
    [MaxLength(200)]
    public string OpenAiApiKey { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string GoogleAiApiKey { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string AnthropicAiApiKey { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string OpenAiModel { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string GoogleAiModel { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string AnthropicAiModel { get; set; } = string.Empty;
}