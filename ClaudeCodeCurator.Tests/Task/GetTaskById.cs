using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.Models;

namespace ClaudeCodeCurator.Tests;

using Commands.UpdateTask;

public partial class IntegrationTests
{
    [Fact]
    public async Task GetTaskById_Returns_Task_When_Exists()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for GetTaskById";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for GetTaskById";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for GetTaskById";
        var promptBody = "Test prompt for retrieval";
        var taskType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            taskType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success
        Assert.True(getTaskResult.IsSuccess);
        
        // Verify the task model has the correct properties
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(taskId, taskModel.Id);
        Assert.Equal(taskName, taskModel.Name);
        Assert.Equal(promptBody, taskModel.PromptBody);
        Assert.Equal(taskType, taskModel.Type);
        Assert.Equal(userStoryId, taskModel.UserStoryId);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Failure_When_Task_Not_Found()
    {
        // Arrange - Use a non-existent task ID
        var nonExistentId = Guid.NewGuid();
        var getTaskRequest = new GetTaskByIdRequest(nonExistentId);
        
        // Act
        var result = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Correct_TaskType_For_Bug()
    {
        // Arrange - Create project, user story, and bug task
        var projectName = "Test Project for Bug Task";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Bug Task";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task with Bug type
        var taskName = "Bug Task";
        var promptBody = "This is a bug report";
        var taskType = TaskType.Bug;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            taskType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the bug task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model has Bug type
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(TaskType.Bug, taskModel.Type);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Correct_TaskType_For_Verify()
    {
        // Arrange - Create project, user story, and verification task
        var projectName = "Test Project for Verify Task";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Verify Task";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task with Verify type
        var taskName = "Verification Task";
        var promptBody = "This is a verification task";
        var taskType = TaskType.Verify;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            taskType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the verify task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model has Verify type
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(TaskType.Verify, taskModel.Type);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Correct_Model_After_Task_Update()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Updated Task";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Updated Task";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task
        var originalName = "Original Task Name";
        var originalPrompt = "Original prompt";
        var originalType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(
            originalName, 
            originalPrompt, 
            userStoryId, 
            originalType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Update the task
        var newName = "Updated Task Name";
        var newPrompt = "Updated prompt text";
        var newType = TaskType.Bug;
        var updateTaskRequest = new UpdateTaskRequest(
            taskId, 
            newName, 
            newPrompt, 
            newType);
            
        var updateTaskResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        Assert.True(updateTaskResult.IsSuccess);
        
        // Act - Get the updated task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model reflects the updates
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(newName, taskModel.Name);
        Assert.Equal(newPrompt, taskModel.PromptBody);
        Assert.Equal(newType, taskModel.Type);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_TaskModel_With_Correct_Structure()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Model Structure";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Model Structure";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Model Structure";
        var promptBody = "Test prompt for model structure";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model has the correct structure
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        
        // Check that the model is of type TaskModel
        Assert.IsType<TaskModel>(taskModel);
        
        // Verify the model has all required properties
        Assert.NotEqual(Guid.Empty, taskModel.Id);
        Assert.NotEmpty(taskModel.Name);
        Assert.NotEmpty(taskModel.PromptBody);
        Assert.NotEqual(Guid.Empty, taskModel.UserStoryId);
        
        // Verify the model doesn't include any entity references
        var taskModelType = typeof(TaskModel);
        var properties = taskModelType.GetProperties();
        
        // Should only contain the 5 expected properties
        Assert.Equal(5, properties.Length);
        
        // Check that there are no navigation properties to other entities
        var propertyNames = properties.Select(p => p.Name).ToList();
        Assert.Contains("Id", propertyNames);
        Assert.Contains("Name", propertyNames);
        Assert.Contains("PromptBody", propertyNames);
        Assert.Contains("Type", propertyNames);
        Assert.Contains("UserStoryId", propertyNames);
        
        // Ensure there are no "UserStory" navigation properties
        Assert.DoesNotContain("UserStory", propertyNames);
    }
}