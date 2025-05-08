namespace ClaudeCodeCurator.Commands.CreateTask;

using System.Threading;
using System.Threading.Tasks;
using ClaudeCodeCurator.Entities;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CreateTaskHandler : IRequestHandler<CreateTaskRequest, Result<Guid>>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateTaskHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<Result<Guid>> Handle(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        try
        {
            // Get the user story and its project to access the counter
            var userStory = await context.UserStories
                .Include(us => us.Project)
                .AsTracking() // Make sure we're tracking entities for updates
                .FirstOrDefaultAsync(us => us.Id == request.UserStoryId, cancellationToken);

            if (userStory == null)
            {
                return Result.Fail($"User story with ID '{request.UserStoryId}' does not exist");
            }

            // Get the project to access and update the counter
            var project = userStory.Project;
            // Ensure the project is being tracked 
            context.Entry(project).State = EntityState.Modified;

            // Check if a task with the same name already exists in the user story
            var existingTask = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.UserStoryId == request.UserStoryId && t.Name == request.Name, 
                    cancellationToken);

            if (existingTask != null)
            {
                return Result.Fail($"Task with name '{request.Name}' already exists in this user story");
            }

            // Increment the counter
            project.TaskNumberCounter++;

            // Create new task with number
            var task = new TaskEntity
            {
                Name = request.Name,
                PromptBody = request.PromptBody,
                UserStoryId = request.UserStoryId,
                Type = request.Type,
                ReferenceUserStory = request.ReferenceUserStory,
                PromptAppendThink = request.PromptAppendThink,
                PromptAppendThinkHard = request.PromptAppendThinkHard,
                PromptAppendDoNotChange = request.PromptAppendDoNotChange,
                UsePrimePrompt = request.UsePrimePrompt,
                UseVerifyPrompt = request.UseVerifyPrompt,
                TaskNumber = project.TaskNumberCounter,
                CreatedOrUpdatedUtc = DateTime.UtcNow
            };

            await context.Tasks.AddAsync(task, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(task.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create task: {ex.Message}");
        }
    }
}