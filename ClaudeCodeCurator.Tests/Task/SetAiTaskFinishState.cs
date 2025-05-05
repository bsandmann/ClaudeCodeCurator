using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Commands.SetAiTaskFinishState;
using ClaudeCodeCurator.Entities;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task SetAiTaskFinishState_Should_Set_FinishedByAiUtc_When_Finished()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for AI Finish";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for AI Finish";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for AI Finish";
        var promptBody = "Test prompt for AI finish";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Get initial task state
        var initialTaskRequest = new GetTaskByIdRequest(taskId);
        var initialTaskResult = await _getTaskByIdHandler.Handle(initialTaskRequest, CancellationToken.None);
        Assert.True(initialTaskResult.IsSuccess);
        var initialTask = initialTaskResult.Value;
        
        // Verify that FinishedByAiUtc is initially null
        Assert.Null(initialTask.FinishedByAiUtc);
        
        // Act - Mark the task as finished by AI
        var finishRequest = new SetAiTaskFinishStateRequest(taskId, true);
        var finishResult = await _setAiTaskFinishStateHandler.Handle(finishRequest, CancellationToken.None);
        
        // Assert - Verify finish was successful
        Assert.True(finishResult.IsSuccess);
        Assert.True(finishResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the FinishedByAiUtc field is set and not null
        Assert.NotNull(updatedTask.FinishedByAiUtc);
        
        // Verify the CreatedOrUpdatedUtc was also updated
        Assert.NotEqual(initialTask.CreatedOrUpdatedUtc, updatedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetAiTaskFinishState_Should_Set_FinishedByAiUtc_To_Null_When_Unfinished()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Clearing AI Finish";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Clearing AI Finish";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Clearing AI Finish";
        var promptBody = "Test prompt for clearing AI finish";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // First, set the finish state
        var setFinishRequest = new SetAiTaskFinishStateRequest(taskId, true);
        var setFinishResult = await _setAiTaskFinishStateHandler.Handle(setFinishRequest, CancellationToken.None);
        Assert.True(setFinishResult.IsSuccess);
        
        // Get task after setting finish state
        var finishedTaskRequest = new GetTaskByIdRequest(taskId);
        var finishedTaskResult = await _getTaskByIdHandler.Handle(finishedTaskRequest, CancellationToken.None);
        Assert.True(finishedTaskResult.IsSuccess);
        var finishedTask = finishedTaskResult.Value;
        
        // Verify that FinishedByAiUtc is set
        Assert.NotNull(finishedTask.FinishedByAiUtc);
        
        // Act - Clear the finish state
        var clearFinishRequest = new SetAiTaskFinishStateRequest(taskId, false);
        var clearFinishResult = await _setAiTaskFinishStateHandler.Handle(clearFinishRequest, CancellationToken.None);
        
        // Assert - Verify clearing was successful
        Assert.True(clearFinishResult.IsSuccess);
        Assert.True(clearFinishResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the FinishedByAiUtc field is null
        Assert.Null(updatedTask.FinishedByAiUtc);
        
        // Verify the CreatedOrUpdatedUtc was also updated
        Assert.NotEqual(finishedTask.CreatedOrUpdatedUtc, updatedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetAiTaskFinishState_Should_Return_False_When_No_Change_Needed()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for No Change in AI Finish";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for No Change in AI Finish";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for No Change in AI Finish";
        var promptBody = "Test prompt for no change in AI finish";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // First, set the finish state
        var firstFinishRequest = new SetAiTaskFinishStateRequest(taskId, true);
        var firstFinishResult = await _setAiTaskFinishStateHandler.Handle(firstFinishRequest, CancellationToken.None);
        Assert.True(firstFinishResult.IsSuccess);
        Assert.True(firstFinishResult.Value); // First change should be true
        
        // Get task state after first finish
        var firstFinishedTaskRequest = new GetTaskByIdRequest(taskId);
        var firstFinishedTaskResult = await _getTaskByIdHandler.Handle(firstFinishedTaskRequest, CancellationToken.None);
        Assert.True(firstFinishedTaskResult.IsSuccess);
        var firstFinishedTask = firstFinishedTaskResult.Value;
        var firstFinishTimestamp = firstFinishedTask.CreatedOrUpdatedUtc;
        
        // Act - Try to set it again (should be no-op)
        var secondFinishRequest = new SetAiTaskFinishStateRequest(taskId, true);
        var secondFinishResult = await _setAiTaskFinishStateHandler.Handle(secondFinishRequest, CancellationToken.None);
        
        // Assert - Verify the result is success but no change was made
        Assert.True(secondFinishResult.IsSuccess);
        Assert.False(secondFinishResult.Value); // Confirm no change was made
        
        // Get task after second finish attempt
        var secondFinishedTaskRequest = new GetTaskByIdRequest(taskId);
        var secondFinishedTaskResult = await _getTaskByIdHandler.Handle(secondFinishedTaskRequest, CancellationToken.None);
        Assert.True(secondFinishedTaskResult.IsSuccess);
        var secondFinishedTask = secondFinishedTaskResult.Value;
        
        // Verify the CreatedOrUpdatedUtc was NOT updated (no change was made)
        Assert.Equal(firstFinishTimestamp, secondFinishedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetAiTaskFinishState_Should_Return_Failure_When_Task_Not_Found()
    {
        // Arrange - Use a non-existent task ID
        var nonExistentId = Guid.NewGuid();
        var finishRequest = new SetAiTaskFinishStateRequest(nonExistentId, true);
        
        // Act
        var result = await _setAiTaskFinishStateHandler.Handle(finishRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
}