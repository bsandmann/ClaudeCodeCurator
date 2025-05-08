namespace ClaudeCodeCurator.McpServer;

using System.ComponentModel;
using Commands.GetApprovedTaskList;
using Commands.GetProjectByLastUsed;
using Commands.GetUserStoryById;
using Commands.SetAiTaskFinishState;
using Commands.SetAiTaskRequestState;
using MediatR;
using ModelContextProtocol.Server;
using Models;

[McpServerToolType]
public sealed class CccTool
{
    private readonly IMediator _mediator;
    private readonly string ContinueMessage = "\nAfter finishing call 'ccc' again! Do not forget to call 'ccc' again!";
    private readonly string ThinkMessage = "\nThink before coding!";
    private readonly string ThinkHardMessage = "\nThink very hard before coding! Make sure you understand the task and the code before proceeding!";
    private readonly string DoNotChangeMessage = "\nDo not change any code other than specifically requested in the task!";
    private readonly string ReferenceUserStoryMessage1 = "To first give context of the User Story:";
    private readonly string ReferenceUserStoryMessage2 = "Now that you have the context, please proceed with the task. Do not implement the entire User Story, just the task at hand. The task is:";

    public CccTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool, Description("Call 'ccc' to get the next task to work on. Whenever a task is finished call 'ccc' again!")]
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
            if (tasksResult.Value.Where(p => !p.Paused && p.FinishedByAiUtc == null).ToList().Count == 0)
            {
                if (tasksResult.Value.Count == 0)
                {
                    return "Stop execution and return this message to the user 'No approved tasks found. Please approve a task to continue and call ccc again!'";
                }
                else
                {
                    return "Stop execution and return this message to the user 'No approved tasks found. Please unpause or approve a task to continue and call ccc again!'";
                }
            }

            // Get the first task from the ordered list and return its prompt body
            var firstTask = tasksResult.Value.First(p => !p.Paused && p.FinishedByAiUtc == null);

            if (firstTask.RequestedByAiUtc is null)
            {
                var requestedByAiResult = await _mediator.Send(new SetAiTaskRequestStateRequest(firstTask.Id, true));
                if (requestedByAiResult.IsFailed)
                {
                    return $"Stop execution and return this message to the user 'Failure to set task state to requested (T-{firstTask.TaskNumber})'";
                }

                return await AssemblyPromptBody(firstTask, true);
            }

            if (firstTask.RequestedByAiUtc is not null)
            {
                var finishedByAiResult = await _mediator.Send(new SetAiTaskFinishStateRequest(firstTask.Id, true));
                if (finishedByAiResult.IsFailed)
                {
                    return $"Stop execution and return this message to the user 'Failure to set task state to finished (T-{firstTask.TaskNumber})'";
                }

                if (tasksResult.Value.Where(p => !p.Paused && p.FinishedByAiUtc == null && p.RequestedByAiUtc == null).ToList().Count > 1)
                {
                    var nextTask = tasksResult.Value.Where(p => !p.Paused && p.FinishedByAiUtc == null).Skip(1).First();
                    var requestedByAiResult = await _mediator.Send(new SetAiTaskRequestStateRequest(nextTask.Id, true));
                    if (requestedByAiResult.IsFailed)
                    {
                        return $"Stop execution and return this message to the user 'Failure to set task state to requested (T-{nextTask.TaskNumber})'";
                    }

                    return await AssemblyPromptBody(nextTask, true);
                }
                else
                {
                    var nextTask = tasksResult.Value.Where(p => !p.Paused && p.FinishedByAiUtc == null).Skip(1).FirstOrDefault();
                    if (nextTask is null)
                    {
                        return $"Stop execution and return this message to the user 'All done'";
                    }

                    var requestedByAiResult = await _mediator.Send(new SetAiTaskRequestStateRequest(nextTask.Id, true));
                    if (requestedByAiResult.IsFailed)
                    {
                        return $"Stop execution and return this message to the user 'Failure to set task state to requested (T-{nextTask.TaskNumber})'";
                    }

                    return await AssemblyPromptBody(nextTask, false);
                }
            }

            return "";
        }
        catch (Exception ex)
        {
            return ("An error occurred while processing your request");
        }
    }

    private async Task<string> AssemblyPromptBody(TaskModel firstTask, bool appendContinueMessage)
    {
        var promptBody = firstTask.PromptBody;
        if (firstTask.ReferenceUserStory)
        {
            var userStory = await _mediator.Send(new GetUserStoryByIdRequest(firstTask.UserStoryId));
            if (userStory.IsFailed)
            {
                return promptBody;
            }

            if (!string.IsNullOrWhiteSpace(userStory.Value.Description))
            {
                promptBody = ReferenceUserStoryMessage1 + "\n" + userStory.Value.Description + ReferenceUserStoryMessage2 + "\n" + promptBody;
            }
        }

        if (firstTask.PromptAppendThink)
        {
            promptBody = promptBody + ThinkMessage;
        }
        else if (firstTask.PromptAppendThinkHard)
        {
            promptBody = promptBody + ThinkHardMessage;
        }

        if (appendContinueMessage)
        {
            return promptBody + ContinueMessage;
        }

        return promptBody;
    }
}