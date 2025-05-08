namespace ClaudeCodeCurator.Commands.UpdateSettings;

using FluentResults;
using MediatR;

public class UpdateSettingsRequest : IRequest<Result<bool>>
{
    public string? OpenAiApiKey { get; }
    public string? GoogleAiApiKey { get; }
    public string? AnthropicAiApiKey { get; }
    public string? OpenAiModel { get; }
    public string? GoogleAiModel { get; }
    public string? AnthropicAiModel { get; }

    public UpdateSettingsRequest(
        string? openAiApiKey = null,
        string? googleAiApiKey = null,
        string? anthropicAiApiKey = null,
        string? openAiModel = null,
        string? googleAiModel = null,
        string? anthropicAiModel = null)
    {
        OpenAiApiKey = openAiApiKey;
        GoogleAiApiKey = googleAiApiKey;
        AnthropicAiApiKey = anthropicAiApiKey;
        OpenAiModel = openAiModel;
        GoogleAiModel = googleAiModel;
        AnthropicAiModel = anthropicAiModel;
    }
}