using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Commands.SetAiTaskRequestState;
using ClaudeCodeCurator.Entities;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task SetAiTaskRequestState_Should_Set_RequestedByAiUtc_When_Requested()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for AI Request";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for AI Request";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for AI Request";
        var promptBody = "Test prompt for AI request";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Get initial task state
        var initialTaskRequest = new GetTaskByIdRequest(taskId);
        var initialTaskResult = await _getTaskByIdHandler.Handle(initialTaskRequest, CancellationToken.None);
        Assert.True(initialTaskResult.IsSuccess);
        var initialTask = initialTaskResult.Value;
        
        // Verify that RequestedByAiUtc is initially null
        Assert.Null(initialTask.RequestedByAiUtc);
        
        // Act - Set the AI request state
        var requestByAiRequest = new SetAiTaskRequestStateRequest(taskId, true);
        var requestByAiResult = await _setAiTaskRequestStateHandler.Handle(requestByAiRequest, CancellationToken.None);
        
        // Assert - Verify request was successful
        Assert.True(requestByAiResult.IsSuccess);
        Assert.True(requestByAiResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the RequestedByAiUtc field is set and not null
        Assert.NotNull(updatedTask.RequestedByAiUtc);
        
        // Verify the CreatedOrUpdatedUtc was also updated
        Assert.NotEqual(initialTask.CreatedOrUpdatedUtc, updatedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetAiTaskRequestState_Should_Set_RequestedByAiUtc_To_Null_When_Cleared()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Clearing AI Request";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Clearing AI Request";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Clearing AI Request";
        var promptBody = "Test prompt for clearing AI request";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // First, set the AI request state
        var setRequestRequest = new SetAiTaskRequestStateRequest(taskId, true);
        var setRequestResult = await _setAiTaskRequestStateHandler.Handle(setRequestRequest, CancellationToken.None);
        Assert.True(setRequestResult.IsSuccess);
        
        // Get task after setting AI request
        var requestedTaskRequest = new GetTaskByIdRequest(taskId);
        var requestedTaskResult = await _getTaskByIdHandler.Handle(requestedTaskRequest, CancellationToken.None);
        Assert.True(requestedTaskResult.IsSuccess);
        var requestedTask = requestedTaskResult.Value;
        
        // Verify that RequestedByAiUtc is set
        Assert.NotNull(requestedTask.RequestedByAiUtc);
        
        // Act - Clear the AI request state
        var clearRequestRequest = new SetAiTaskRequestStateRequest(taskId, false);
        var clearRequestResult = await _setAiTaskRequestStateHandler.Handle(clearRequestRequest, CancellationToken.None);
        
        // Assert - Verify clearing was successful
        Assert.True(clearRequestResult.IsSuccess);
        Assert.True(clearRequestResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the RequestedByAiUtc field is null
        Assert.Null(updatedTask.RequestedByAiUtc);
        
        // Verify the CreatedOrUpdatedUtc was also updated
        Assert.NotEqual(requestedTask.CreatedOrUpdatedUtc, updatedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetAiTaskRequestState_Should_Return_False_When_No_Change_Needed()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for No Change in AI Request";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for No Change in AI Request";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for No Change in AI Request";
        var promptBody = "Test prompt for no change in AI request";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // First, set the AI request state
        var firstRequestRequest = new SetAiTaskRequestStateRequest(taskId, true);
        var firstRequestResult = await _setAiTaskRequestStateHandler.Handle(firstRequestRequest, CancellationToken.None);
        Assert.True(firstRequestResult.IsSuccess);
        Assert.True(firstRequestResult.Value); // First change should be true
        
        // Get task state after first request
        var firstRequestedTaskRequest = new GetTaskByIdRequest(taskId);
        var firstRequestedTaskResult = await _getTaskByIdHandler.Handle(firstRequestedTaskRequest, CancellationToken.None);
        Assert.True(firstRequestedTaskResult.IsSuccess);
        var firstRequestedTask = firstRequestedTaskResult.Value;
        var firstRequestTimestamp = firstRequestedTask.CreatedOrUpdatedUtc;
        
        // Act - Try to set it again (should be no-op)
        var secondRequestRequest = new SetAiTaskRequestStateRequest(taskId, true);
        var secondRequestResult = await _setAiTaskRequestStateHandler.Handle(secondRequestRequest, CancellationToken.None);
        
        // Assert - Verify the result is success but no change was made
        Assert.True(secondRequestResult.IsSuccess);
        Assert.False(secondRequestResult.Value); // Confirm no change was made
        
        // Get task after second request attempt
        var secondRequestedTaskRequest = new GetTaskByIdRequest(taskId);
        var secondRequestedTaskResult = await _getTaskByIdHandler.Handle(secondRequestedTaskRequest, CancellationToken.None);
        Assert.True(secondRequestedTaskResult.IsSuccess);
        var secondRequestedTask = secondRequestedTaskResult.Value;
        
        // Verify the CreatedOrUpdatedUtc was NOT updated (no change was made)
        Assert.Equal(firstRequestTimestamp, secondRequestedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetAiTaskRequestState_Should_Return_Failure_When_Task_Not_Found()
    {
        // Arrange - Use a non-existent task ID
        var nonExistentId = Guid.NewGuid();
        var requestByAiRequest = new SetAiTaskRequestStateRequest(nonExistentId, true);
        
        // Act
        var result = await _setAiTaskRequestStateHandler.Handle(requestByAiRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
}