using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetApprovedTaskList;
using ClaudeCodeCurator.Commands.SetUserTaskApproval;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.Models;
using FluentAssertions;
using System.Linq;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task GetApprovedTaskList_Returns_Empty_List_When_No_Approved_Tasks()
    {
        // Arrange - Create project, user story, and tasks (but don't approve them)
        var projectName = "Test Project for GetApprovedTaskList";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for GetApprovedTaskList";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create tasks but don't approve them
        var task1Request = new CreateTaskRequest("Task 1", "Prompt 1", userStoryId);
        var task2Request = new CreateTaskRequest("Task 2", "Prompt 2", userStoryId);
        
        await _createTaskHandler.Handle(task1Request, CancellationToken.None);
        await _createTaskHandler.Handle(task2Request, CancellationToken.None);
        
        // Create handler for the GetApprovedTaskList command
        var getApprovedTaskListHandler = new GetApprovedTaskListHandler(_serviceScopeFactoryMock.Object);
        
        // Act - Get approved tasks for the project
        var getApprovedTaskListRequest = new GetApprovedTaskListRequest(projectId);
        var result = await getApprovedTaskListHandler.Handle(getApprovedTaskListRequest, CancellationToken.None);
        
        // Assert - Verify result is successful but list is empty
        Assert.True(result.IsSuccess);
        result.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetApprovedTaskList_Returns_Approved_Tasks_In_Correct_Order()
    {
        // Arrange - Create project, user story, and tasks
        var projectName = "Test Project for Approved Tasks";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Approved Tasks";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create tasks
        var task1Request = new CreateTaskRequest("Task 1", "Prompt 1", userStoryId);
        var task2Request = new CreateTaskRequest("Task 2", "Prompt 2", userStoryId);
        var task3Request = new CreateTaskRequest("Task 3", "Prompt 3", userStoryId);
        
        var task1Result = await _createTaskHandler.Handle(task1Request, CancellationToken.None);
        var task2Result = await _createTaskHandler.Handle(task2Request, CancellationToken.None);
        var task3Result = await _createTaskHandler.Handle(task3Request, CancellationToken.None);
        
        Assert.True(task1Result.IsSuccess);
        Assert.True(task2Result.IsSuccess);
        Assert.True(task3Result.IsSuccess);
        
        var task1Id = task1Result.Value;
        var task2Id = task2Result.Value;
        var task3Id = task3Result.Value;
        
        // Approve tasks 1 and 3, but not task 2
        var approveTask1Request = new SetUserTaskApprovalRequest(task1Id, true, projectId);
        var approveTask3Request = new SetUserTaskApprovalRequest(task3Id, true, projectId);
        
        await _setUserTaskApprovalHandler.Handle(approveTask1Request, CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(approveTask3Request, CancellationToken.None);
        
        // Create handler for the GetApprovedTaskList command
        var getApprovedTaskListHandler = new GetApprovedTaskListHandler(_serviceScopeFactoryMock.Object);
        
        // Act - Get approved tasks for the project
        var getApprovedTaskListRequest = new GetApprovedTaskListRequest(projectId);
        var result = await getApprovedTaskListHandler.Handle(getApprovedTaskListRequest, CancellationToken.None);
        
        // Assert - Verify result contains only approved tasks in correct order
        Assert.True(result.IsSuccess);
        result.Value.Should().HaveCount(2);
        
        // Order should be based on the order in ProjectTaskOrders
        var taskIds = result.Value.Select(t => t.Id).ToList();
        taskIds.Should().Contain(task1Id);
        taskIds.Should().Contain(task3Id);
        taskIds.Should().NotContain(task2Id);
        
        // The first task approved should be first in the list (position 100)
        // The second task approved should be second in the list (position 200)
        result.Value[0].Id.Should().Be(task1Id);
        result.Value[1].Id.Should().Be(task3Id);
    }
    
    [Fact]
    public async Task GetApprovedTaskList_Returns_Error_For_NonExistent_Project()
    {
        // Arrange - Use a non-existent project ID
        var nonExistentProjectId = Guid.NewGuid();
        
        // Create handler for the GetApprovedTaskList command
        var getApprovedTaskListHandler = new GetApprovedTaskListHandler(_serviceScopeFactoryMock.Object);
        
        // Act - Get approved tasks for the non-existent project
        var getApprovedTaskListRequest = new GetApprovedTaskListRequest(nonExistentProjectId);
        var result = await getApprovedTaskListHandler.Handle(getApprovedTaskListRequest, CancellationToken.None);
        
        // Assert - Verify result is a failure with correct error message
        Assert.False(result.IsSuccess);
        result.Errors.First().Message.Should().Contain($"Project with ID '{nonExistentProjectId}'");
    }
    
    [Fact]
    public async Task GetApprovedTaskList_Handles_Task_Approval_Status_Changes()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Approval Status Changes";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Approval Status Changes";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task
        var taskRequest = new CreateTaskRequest("Approval Change Task", "Prompt", userStoryId);
        var taskResult = await _createTaskHandler.Handle(taskRequest, CancellationToken.None);
        
        Assert.True(taskResult.IsSuccess);
        var taskId = taskResult.Value;
        
        // Create handler for the GetApprovedTaskList command
        var getApprovedTaskListHandler = new GetApprovedTaskListHandler(_serviceScopeFactoryMock.Object);
        var getApprovedTaskListRequest = new GetApprovedTaskListRequest(projectId);
        
        // Verify initially no approved tasks
        var initialResult = await getApprovedTaskListHandler.Handle(getApprovedTaskListRequest, CancellationToken.None);
        Assert.True(initialResult.IsSuccess);
        initialResult.Value.Should().BeEmpty();
        
        // Approve the task
        var approveTaskRequest = new SetUserTaskApprovalRequest(taskId, true, projectId);
        await _setUserTaskApprovalHandler.Handle(approveTaskRequest, CancellationToken.None);
        
        // Verify task is now in the approved list
        var afterApprovalResult = await getApprovedTaskListHandler.Handle(getApprovedTaskListRequest, CancellationToken.None);
        Assert.True(afterApprovalResult.IsSuccess);
        afterApprovalResult.Value.Should().HaveCount(1);
        afterApprovalResult.Value[0].Id.Should().Be(taskId);
        
        // Unapprove the task
        var unapproveTaskRequest = new SetUserTaskApprovalRequest(taskId, false, projectId);
        await _setUserTaskApprovalHandler.Handle(unapproveTaskRequest, CancellationToken.None);
        
        // Verify task is no longer in the approved list
        var afterUnapprovalResult = await getApprovedTaskListHandler.Handle(getApprovedTaskListRequest, CancellationToken.None);
        Assert.True(afterUnapprovalResult.IsSuccess);
        afterUnapprovalResult.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetApprovedTaskList_Returns_Tasks_With_PrimePrompt_And_VerifyPrompt_Flags()
    {
        // Arrange - Create project, user story
        var projectName = "Test Project for Prime and Verify Flags";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Prime and Verify Flags";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create tasks with UsePrimePrompt and UseVerifyPrompt flags set
        var createTaskPrimeRequest = new CreateTaskRequest(
            "Prime Task", 
            "Prompt with prime", 
            userStoryId,
            TaskType.Task,
            false,  // ReferenceUserStory
            false,  // PromptAppendThink
            false,  // PromptAppendThinkHard
            false,  // PromptAppendDoNotChange
            true,   // UsePrimePrompt
            false); // UseVerifyPrompt
            
        var createTaskVerifyRequest = new CreateTaskRequest(
            "Verify Task", 
            "Prompt with verify", 
            userStoryId,
            TaskType.Task,
            false,  // ReferenceUserStory
            false,  // PromptAppendThink
            false,  // PromptAppendThinkHard
            false,  // PromptAppendDoNotChange
            false,  // UsePrimePrompt
            true);  // UseVerifyPrompt
            
        var taskPrimeResult = await _createTaskHandler.Handle(createTaskPrimeRequest, CancellationToken.None);
        var taskVerifyResult = await _createTaskHandler.Handle(createTaskVerifyRequest, CancellationToken.None);
        
        Assert.True(taskPrimeResult.IsSuccess);
        Assert.True(taskVerifyResult.IsSuccess);
        
        var taskPrimeId = taskPrimeResult.Value;
        var taskVerifyId = taskVerifyResult.Value;
        
        // Approve both tasks
        var approveTaskPrimeRequest = new SetUserTaskApprovalRequest(taskPrimeId, true, projectId);
        var approveTaskVerifyRequest = new SetUserTaskApprovalRequest(taskVerifyId, true, projectId);
        
        await _setUserTaskApprovalHandler.Handle(approveTaskPrimeRequest, CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(approveTaskVerifyRequest, CancellationToken.None);
        
        // Act - Get approved tasks for the project
        var getApprovedTaskListHandler = new GetApprovedTaskListHandler(_serviceScopeFactoryMock.Object);
        var getApprovedTaskListRequest = new GetApprovedTaskListRequest(projectId);
        var result = await getApprovedTaskListHandler.Handle(getApprovedTaskListRequest, CancellationToken.None);
        
        // Assert - Verify tasks are returned with correct flag values
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        
        var primeTask = result.Value.FirstOrDefault(t => t.Id == taskPrimeId);
        var verifyTask = result.Value.FirstOrDefault(t => t.Id == taskVerifyId);
        
        Assert.NotNull(primeTask);
        Assert.NotNull(verifyTask);
        
        // Verify flags are set correctly
        Assert.True(primeTask.UsePrimePrompt);
        Assert.False(primeTask.UseVerifyPrompt);
        
        Assert.False(verifyTask.UsePrimePrompt);
        Assert.True(verifyTask.UseVerifyPrompt);
    }
}