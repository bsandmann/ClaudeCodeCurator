using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Commands.RemoveTask;
using ClaudeCodeCurator.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task RemoveTask_Succeeds_For_Existing_Task()
    {
        // Arrange - Create a project, user story, and task
        var projectName = "RemoveTask Test Project 1";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveTask Test Story 1";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "RemoveTask Test Task 1";
        var promptBody = "Task to be removed";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Verify the task exists before removal
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        Assert.True(getTaskResult.IsSuccess);
        
        // Act - Remove the task
        var removeTaskRequest = new RemoveTaskRequest(taskId);
        var removeTaskResult = await _removeTaskHandler.Handle(removeTaskRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeTaskResult.IsSuccess);
        Assert.True(removeTaskResult.Value);
        
        // Verify the task no longer exists in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var task = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.Null(task);
        }
    }
    
    [Fact]
    public async Task RemoveTask_Fails_For_NonExistent_Task()
    {
        // Arrange - Use a non-existent task ID
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var removeTaskRequest = new RemoveTaskRequest(nonExistentId);
        var result = await _removeTaskHandler.Handle(removeTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task RemoveTask_Does_Not_Affect_Other_Tasks()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveTask Test Project 2";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveTask Test Story 2";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create two tasks
        var taskName1 = "RemoveTask Test Task 2";
        var taskName2 = "RemoveTask Test Task 3";
        
        var createTask1Request = new CreateTaskRequest(taskName1, "Task to be removed", userStoryId);
        var createTask2Request = new CreateTaskRequest(taskName2, "Task to remain", userStoryId);
        
        var task1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        var task2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        
        Assert.True(task1Result.IsSuccess);
        Assert.True(task2Result.IsSuccess);
        
        var task1Id = task1Result.Value;
        var task2Id = task2Result.Value;
        
        // Act - Remove only the first task
        var removeTaskRequest = new RemoveTaskRequest(task1Id);
        var removeTaskResult = await _removeTaskHandler.Handle(removeTaskRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeTaskResult.IsSuccess);
        
        // Verify the first task no longer exists
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var task1 = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == task1Id);
                
            Assert.Null(task1);
            
            // Verify the second task still exists
            var task2 = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == task2Id);
                
            Assert.NotNull(task2);
            Assert.Equal(taskName2, task2.Name);
        }
    }
    
    [Fact]
    public async Task RemoveTask_Does_Not_Affect_Parent_UserStory()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveTask Test Project 3";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveTask Test Story 3";
        var userStoryDescription = "User story that should remain after task removal";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId, userStoryDescription);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task
        var taskName = "RemoveTask Test Task 4";
        var createTaskRequest = new CreateTaskRequest(taskName, "Task to be removed", userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Remove the task
        var removeTaskRequest = new RemoveTaskRequest(taskId);
        var removeTaskResult = await _removeTaskHandler.Handle(removeTaskRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeTaskResult.IsSuccess);
        
        // Verify the task no longer exists
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var task = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.Null(task);
            
            // Verify the user story still exists and is unchanged
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.NotNull(userStory);
            Assert.Equal(userStoryName, userStory.Name);
            Assert.Equal(userStoryDescription, userStory.Description);
            Assert.Equal(projectId, userStory.ProjectId);
        }
    }
    
    [Fact]
    public async Task RemoveTask_Can_Remove_All_Tasks_From_UserStory()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveTask Test Project 4";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveTask Test Story 4";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create multiple tasks
        var taskIds = new List<Guid>();
        for (int i = 1; i <= 3; i++)
        {
            var createTaskRequest = new CreateTaskRequest(
                $"RemoveTask Test Multiple Task {i}", 
                $"Task {i} to be removed", 
                userStoryId);
                
            var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
            Assert.True(createTaskResult.IsSuccess);
            taskIds.Add(createTaskResult.Value);
        }
        
        // Act - Remove all tasks one by one
        foreach (var taskId in taskIds)
        {
            var removeTaskRequest = new RemoveTaskRequest(taskId);
            var removeTaskResult = await _removeTaskHandler.Handle(removeTaskRequest, CancellationToken.None);
            Assert.True(removeTaskResult.IsSuccess);
        }
        
        // Assert - Verify all tasks were removed
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Verify no tasks exist for this user story
            var tasksCount = await context.Tasks
                .AsNoTracking()
                .Where(t => t.UserStoryId == userStoryId)
                .CountAsync();
                
            Assert.Equal(0, tasksCount);
            
            // Verify the user story still exists
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.NotNull(userStory);
        }
    }
}