namespace ClaudeCodeCurator.McpServer;

using System.ComponentModel;
using Commands.GetApprovedTaskList;
using Commands.GetProjectByLastUsed;
using Commands.SetAiTaskFinishState;
using Commands.SetAiTaskRequestState;
using MediatR;
using ModelContextProtocol.Server;

[McpServerToolType]
public sealed class CccTool
{
    private readonly IMediator _mediator;
    private readonly string ContinueMessage = "After finishing call 'ccc' again!";

    public CccTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool, Description("ccc")]
    public async Task<string> Ccc()
    {
        try
        {
            // Get the most recently used project first
            var projectResult = await _mediator.Send(new GetProjectByLastUsedRequest());

            if (!projectResult.IsSuccess)
            {
                return "No project found";
            }

            // Get the approved tasks for this project
            var tasksResult = await _mediator.Send(new GetApprovedTaskListRequest(projectResult.Value.Id));

            if (!tasksResult.IsSuccess)
            {
                return "Stop execution and return this message to the user 'Failure to get approved tasks'";
            }

            // If there are no approved tasks, return a 404
            if (tasksResult.Value.Count == 0)
            {
                return "Stop execution and return this message to the user 'No approved tasks found. Please approve a task to continue and call ccc again!'";
            }

            // Get the first task from the ordered list and return its prompt body
            var firstTask = tasksResult.Value.First();

            if (firstTask.RequestedByAiUtc is null)
            {
                var requestedByAiResult = await _mediator.Send(new SetAiTaskRequestStateRequest(firstTask.Id, true));
                if (requestedByAiResult.IsFailed)
                {
                    return $"Stop execution and return this message to the user 'Failure to set task state to requested (T-{firstTask.TaskNumber})'";
                }

                return firstTask.PromptBody + "\n\n" + ContinueMessage;
            }

            if (firstTask.RequestedByAiUtc is not null)
            {
                var finishedByAiResult = await _mediator.Send(new SetAiTaskFinishStateRequest(firstTask.Id, true));
                if (finishedByAiResult.IsFailed)
                {
                    return $"Stop execution and return this message to the user 'Failure to set task state to finished (T-{firstTask.TaskNumber})'";
                }

                if (tasksResult.Value.Count > 1)
                {
                    var nextTask = tasksResult.Value.Skip(1).First();
                    var requestedByAiResult = await _mediator.Send(new SetAiTaskRequestStateRequest(nextTask.Id, true));
                    if (requestedByAiResult.IsFailed)
                    {
                        return $"Stop execution and return this message to the user 'Failure to set task state to requested (T-{nextTask.TaskNumber})'";
                    }

                    return nextTask.PromptBody + "\n\n" + ContinueMessage;
                }
                else
                {
                    var nextTask = tasksResult.Value.Skip(1).First();
                    var requestedByAiResult = await _mediator.Send(new SetAiTaskRequestStateRequest(nextTask.Id, true));
                    if (requestedByAiResult.IsFailed)
                    {
                        return $"Stop execution and return this message to the user 'Failure to set task state to requested (T-{nextTask.TaskNumber})'";
                    }

                    return nextTask.PromptBody;
                }
            }

            return "";
        }
        catch (Exception ex)
        {
            return ("An error occurred while processing your request");
        }
    }
}