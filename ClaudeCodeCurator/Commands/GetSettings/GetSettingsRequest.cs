namespace ClaudeCodeCurator.Commands.GetSettings;

using ClaudeCodeCurator.Models;
using FluentResults;
using MediatR;

public class GetSettingsRequest : IRequest<Result<SettingsModel>>
{
    // This request takes no parameters
}