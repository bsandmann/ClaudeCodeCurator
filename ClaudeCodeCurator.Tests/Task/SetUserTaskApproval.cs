using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Commands.SetUserTaskApproval;
using ClaudeCodeCurator.Entities;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task SetUserTaskApproval_Should_Set_ApprovedByUserUtc_When_Approved()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Approval";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Approval";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Approval";
        var promptBody = "Test prompt for approval";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Get initial task state
        var initialTaskRequest = new GetTaskByIdRequest(taskId);
        var initialTaskResult = await _getTaskByIdHandler.Handle(initialTaskRequest, CancellationToken.None);
        Assert.True(initialTaskResult.IsSuccess);
        var initialTask = initialTaskResult.Value;
        
        // Verify that ApprovedByUserUtc is initially null
        Assert.Null(initialTask.ApprovedByUserUtc);
        
        // Act - Approve the task
        var approveRequest = new SetUserTaskApprovalRequest(taskId, true);
        var approveResult = await _setUserTaskApprovalHandler.Handle(approveRequest, CancellationToken.None);
        
        // Assert - Verify approval was successful
        Assert.True(approveResult.IsSuccess);
        Assert.True(approveResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the ApprovedByUserUtc field is set and not null
        Assert.NotNull(updatedTask.ApprovedByUserUtc);
        
        // Verify the CreatedOrUpdatedUtc was also updated
        Assert.NotEqual(initialTask.CreatedOrUpdatedUtc, updatedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetUserTaskApproval_Should_Set_ApprovedByUserUtc_To_Null_When_Not_Approved()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Un-Approval";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Un-Approval";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Un-Approval";
        var promptBody = "Test prompt for un-approval";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // First, approve the task
        var approveRequest = new SetUserTaskApprovalRequest(taskId, true);
        var approveResult = await _setUserTaskApprovalHandler.Handle(approveRequest, CancellationToken.None);
        Assert.True(approveResult.IsSuccess);
        
        // Get task after approval
        var approvedTaskRequest = new GetTaskByIdRequest(taskId);
        var approvedTaskResult = await _getTaskByIdHandler.Handle(approvedTaskRequest, CancellationToken.None);
        Assert.True(approvedTaskResult.IsSuccess);
        var approvedTask = approvedTaskResult.Value;
        
        // Verify that ApprovedByUserUtc is set
        Assert.NotNull(approvedTask.ApprovedByUserUtc);
        
        // Act - Un-approve the task
        var unapproveRequest = new SetUserTaskApprovalRequest(taskId, false);
        var unapproveResult = await _setUserTaskApprovalHandler.Handle(unapproveRequest, CancellationToken.None);
        
        // Assert - Verify un-approval was successful
        Assert.True(unapproveResult.IsSuccess);
        Assert.True(unapproveResult.Value); // Confirm the change was made
        
        // Get updated task
        var updatedTaskRequest = new GetTaskByIdRequest(taskId);
        var updatedTaskResult = await _getTaskByIdHandler.Handle(updatedTaskRequest, CancellationToken.None);
        Assert.True(updatedTaskResult.IsSuccess);
        var updatedTask = updatedTaskResult.Value;
        
        // Verify the ApprovedByUserUtc field is null
        Assert.Null(updatedTask.ApprovedByUserUtc);
        
        // Verify the CreatedOrUpdatedUtc was also updated
        Assert.NotEqual(approvedTask.CreatedOrUpdatedUtc, updatedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetUserTaskApproval_Should_Return_False_When_No_Change_Needed()
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
        
        // First, approve the task
        var approveRequest = new SetUserTaskApprovalRequest(taskId, true);
        var approveResult = await _setUserTaskApprovalHandler.Handle(approveRequest, CancellationToken.None);
        Assert.True(approveResult.IsSuccess);
        Assert.True(approveResult.Value); // First change should be true
        
        // Get task state after first approval
        var firstApprovedTaskRequest = new GetTaskByIdRequest(taskId);
        var firstApprovedTaskResult = await _getTaskByIdHandler.Handle(firstApprovedTaskRequest, CancellationToken.None);
        Assert.True(firstApprovedTaskResult.IsSuccess);
        var firstApprovedTask = firstApprovedTaskResult.Value;
        var firstApprovalTimestamp = firstApprovedTask.CreatedOrUpdatedUtc;
        
        // Act - Try to approve it again (should be no-op)
        var secondApproveRequest = new SetUserTaskApprovalRequest(taskId, true);
        var secondApproveResult = await _setUserTaskApprovalHandler.Handle(secondApproveRequest, CancellationToken.None);
        
        // Assert - Verify the result is success but no change was made
        Assert.True(secondApproveResult.IsSuccess);
        Assert.False(secondApproveResult.Value); // Confirm no change was made
        
        // Get task after second approval attempt
        var secondApprovedTaskRequest = new GetTaskByIdRequest(taskId);
        var secondApprovedTaskResult = await _getTaskByIdHandler.Handle(secondApprovedTaskRequest, CancellationToken.None);
        Assert.True(secondApprovedTaskResult.IsSuccess);
        var secondApprovedTask = secondApprovedTaskResult.Value;
        
        // Verify the CreatedOrUpdatedUtc was NOT updated (no change was made)
        Assert.Equal(firstApprovalTimestamp, secondApprovedTask.CreatedOrUpdatedUtc);
    }
    
    [Fact]
    public async Task SetUserTaskApproval_Should_Return_Failure_When_Task_Not_Found()
    {
        // Arrange - Use a non-existent task ID
        var nonExistentId = Guid.NewGuid();
        var approveRequest = new SetUserTaskApprovalRequest(nonExistentId, true);
        
        // Act
        var result = await _setUserTaskApprovalHandler.Handle(approveRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
}