namespace ClaudeCodeCurator.Controllers;

using System;
using System.Linq;
using System.Threading.Tasks;
using ClaudeCodeCurator.Commands.GetApprovedTaskList;
using ClaudeCodeCurator.Commands.GetProjectByLastUsed;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class McpController : ControllerBase
{
    private readonly ILogger<McpController> _logger;
    private readonly IMediator _mediator;

    public McpController(ILogger<McpController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("GetNextTask")]
    public async Task<IActionResult> GetNextTask()
    {
        _logger.LogInformation("GetNextTask API endpoint called");
        
        try
        {
            // Get the most recently used project first
            var projectResult = await _mediator.Send(new GetProjectByLastUsedRequest());
            
            if (!projectResult.IsSuccess)
            {
                _logger.LogWarning($"Failed to get last used project: {string.Join(", ", projectResult.Errors.Select(e => e.Message))}");
                return NotFound("No project found");
            }
            
            // Get the approved tasks for this project
            var tasksResult = await _mediator.Send(new GetApprovedTaskListRequest(projectResult.Value.Id));
            
            if (!tasksResult.IsSuccess)
            {
                _logger.LogWarning($"Failed to get approved tasks: {string.Join(", ", tasksResult.Errors.Select(e => e.Message))}");
                return NotFound("No approved tasks found");
            }
            
            // If there are no approved tasks, return a 404
            if (tasksResult.Value.Count == 0)
            {
                _logger.LogInformation("No approved tasks found for the project");
                return NotFound("No approved tasks found");
            }
            
            // Get the first task from the ordered list and return its prompt body
            var firstTask = tasksResult.Value.First();
            _logger.LogInformation($"Returning prompt body for task {firstTask.Name} (ID: {firstTask.Id})");
            
            return Ok(firstTask.PromptBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next task");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}