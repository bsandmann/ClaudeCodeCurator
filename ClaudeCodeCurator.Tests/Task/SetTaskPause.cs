using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Commands.SetTaskPause;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task SetTaskPause_Should_Pause_Task_When_Not_Paused()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Pause";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Pause";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Pause";
        var promptBody = "Test prompt for pause";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Get initial task state
        var initialTaskRequest = new GetTaskByIdRequest(taskId);
        var initialTaskResult = await _getTaskByIdHandler.Handle(initialTaskRequest, CancellationToken.None);
        Assert.True(initialTaskResult.IsSuccess);
        var initialTask = initialTaskResult.Value;
        
        // Verify that Paused is initially false (default)
        Assert.False(initialTask.Paused);
        
        // Act - Pause the task
        var setTaskPauseHandler = _serviceProvider.GetRequiredService<SetTaskPauseHandler>();
        var pauseRequest = new SetTaskPauseRequest(taskId, true);
        var pauseResult = await setTaskPauseHandler.Handle(pauseRequest, CancellationToken.None);
        
        // Assert - Verify pause was successful
        Assert.True(pauseResult.IsSuccess);
        Assert.True(pauseResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the Paused field is now true
        Assert.True(updatedTask.Paused);
        
        // Verify the CreatedOrUpdatedUtc was also updated
        Assert.NotEqual(initialTask.CreatedOrUpdatedUtc, updatedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetTaskPause_Should_Unpause_Task_When_Paused()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Unpause";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Unpause";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Unpause";
        var promptBody = "Test prompt for unpause";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // First, pause the task
        var setTaskPauseHandler = _serviceProvider.GetRequiredService<SetTaskPauseHandler>();
        var pauseRequest = new SetTaskPauseRequest(taskId, true);
        var pauseResult = await setTaskPauseHandler.Handle(pauseRequest, CancellationToken.None);
        Assert.True(pauseResult.IsSuccess);
        
        // Get task after pausing to verify it's paused
        var pausedTaskRequest = new GetTaskByIdRequest(taskId);
        var pausedTaskResult = await _getTaskByIdHandler.Handle(pausedTaskRequest, CancellationToken.None);
        Assert.True(pausedTaskResult.IsSuccess);
        var pausedTask = pausedTaskResult.Value;
        Assert.True(pausedTask.Paused);
        
        // Act - Unpause the task
        var unpauseRequest = new SetTaskPauseRequest(taskId, false);
        var unpauseResult = await setTaskPauseHandler.Handle(unpauseRequest, CancellationToken.None);
        
        // Assert - Verify unpause was successful
        Assert.True(unpauseResult.IsSuccess);
        Assert.True(unpauseResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the Paused field is now false
        Assert.False(updatedTask.Paused);
    }
    
    [Fact]
    public async Task SetTaskPause_Should_Return_False_When_No_Change_Needed()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for No Change";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for No Change";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for No Change";
        var promptBody = "Test prompt for no change";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Get initial state (should be unpaused by default)
        var initialTaskRequest = new GetTaskByIdRequest(taskId);
        var initialTaskResult = await _getTaskByIdHandler.Handle(initialTaskRequest, CancellationToken.None);
        Assert.True(initialTaskResult.IsSuccess);
        var initialTask = initialTaskResult.Value;
        Assert.False(initialTask.Paused);
        var initialTimestamp = initialTask.CreatedOrUpdatedUtc;
        
        // Act - Try to set to false again (should be no-op)
        var setTaskPauseHandler = _serviceProvider.GetRequiredService<SetTaskPauseHandler>();
        var request = new SetTaskPauseRequest(taskId, false);
        var result = await setTaskPauseHandler.Handle(request, CancellationToken.None);
        
        // Assert - Verify the result is success but no change was made
        Assert.True(result.IsSuccess);
        Assert.False(result.Value); // No change was made
        
        // Verify timestamp was not updated
        var finalTaskRequest = new GetTaskByIdRequest(taskId);
        var finalTaskResult = await _getTaskByIdHandler.Handle(finalTaskRequest, CancellationToken.None);
        Assert.True(finalTaskResult.IsSuccess);
        var finalTask = finalTaskResult.Value;
        Assert.Equal(initialTimestamp, finalTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetTaskPause_Should_Return_Failure_When_Task_Not_Found()
    {
        // Arrange - Use a non-existent task ID
        var nonExistentId = Guid.NewGuid();
        var setTaskPauseHandler = _serviceProvider.GetRequiredService<SetTaskPauseHandler>();
        var request = new SetTaskPauseRequest(nonExistentId, true);
        
        // Act
        var result = await setTaskPauseHandler.Handle(request, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
}