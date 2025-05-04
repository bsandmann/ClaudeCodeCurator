using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetUserStoryById;
using ClaudeCodeCurator.Commands.UpdateUserStory;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.Models;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task GetUserStoryById_Returns_UserStory_When_Exists()
    {
        // Arrange - Create project and user story
        var projectName = "GUSB Test Project 1";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "GUSB Test Story 1";
        var userStoryDescription = "User story for testing GetUserStoryById";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId, userStoryDescription);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Act - Get the user story by ID
        var getUserStoryRequest = new GetUserStoryByIdRequest(userStoryId);
        var getUserStoryResult = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success
        Assert.True(getUserStoryResult.IsSuccess);
        
        // Verify the user story model has the correct properties
        var userStoryModel = getUserStoryResult.Value;
        Assert.NotNull(userStoryModel);
        Assert.Equal(userStoryId, userStoryModel.Id);
        Assert.Equal(userStoryName, userStoryModel.Name);
        Assert.Equal(userStoryDescription, userStoryModel.Description);
        Assert.Equal(projectId, userStoryModel.ProjectId);
        Assert.Empty(userStoryModel.Tasks); // Should be empty as we haven't created any tasks
    }
    
    [Fact]
    public async Task GetUserStoryById_Returns_Failure_When_UserStory_Not_Found()
    {
        // Arrange - Use a non-existent user story ID
        var nonExistentId = Guid.NewGuid();
        var getUserStoryRequest = new GetUserStoryByIdRequest(nonExistentId);
        
        // Act
        var result = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"User story with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task GetUserStoryById_Returns_UserStory_With_Tasks_When_Included()
    {
        // Arrange - Create project and user story
        var projectName = "GUSB Test Project 2";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "GUSB Test Story 2";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create multiple tasks for this user story
        var task1Name = "GUSB Task 1";
        var task2Name = "GUSB Task 2";
        var task3Name = "GUSB Task 3";
        
        var createTask1Request = new CreateTaskRequest(task1Name, "Prompt 1", userStoryId, TaskType.Task);
        var createTask2Request = new CreateTaskRequest(task2Name, "Prompt 2", userStoryId, TaskType.Bug);
        var createTask3Request = new CreateTaskRequest(task3Name, "Prompt 3", userStoryId, TaskType.Verify);
        
        var task1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        var task2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        var task3Result = await _createTaskHandler.Handle(createTask3Request, CancellationToken.None);
        
        Assert.True(task1Result.IsSuccess);
        Assert.True(task2Result.IsSuccess);
        Assert.True(task3Result.IsSuccess);
        
        // Act - Get the user story by ID with tasks included (default behavior)
        var getUserStoryRequest = new GetUserStoryByIdRequest(userStoryId, excludeTasks: false);
        var getUserStoryResult = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success with tasks
        Assert.True(getUserStoryResult.IsSuccess);
        var userStoryModel = getUserStoryResult.Value;
        Assert.NotNull(userStoryModel);
        
        // Verify tasks are included and have the correct properties
        Assert.Equal(3, userStoryModel.Tasks.Count);
        
        // Verify tasks have the expected names
        var taskNames = userStoryModel.Tasks.Select(t => t.Name).ToList();
        Assert.Contains(task1Name, taskNames);
        Assert.Contains(task2Name, taskNames);
        Assert.Contains(task3Name, taskNames);
        
        // Verify tasks have the correct user story ID
        foreach (var task in userStoryModel.Tasks)
        {
            Assert.Equal(userStoryId, task.UserStoryId);
        }
        
        // Verify tasks have the correct types
        Assert.Contains(userStoryModel.Tasks, t => t.Type == TaskType.Task);
        Assert.Contains(userStoryModel.Tasks, t => t.Type == TaskType.Bug);
        Assert.Contains(userStoryModel.Tasks, t => t.Type == TaskType.Verify);
    }
    
    [Fact]
    public async Task GetUserStoryById_Returns_UserStory_Without_Tasks_When_Excluded()
    {
        // Arrange - Create project and user story
        var projectName = "GUSB Test Project 3";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "GUSB Test Story 3";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task for this user story
        var taskName = "GUSB Task 4";
        var createTaskRequest = new CreateTaskRequest(taskName, "Prompt 4", userStoryId);
        var taskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(taskResult.IsSuccess);
        
        // Act - Get the user story by ID with tasks explicitly excluded
        var getUserStoryRequest = new GetUserStoryByIdRequest(userStoryId, excludeTasks: true);
        var getUserStoryResult = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success without tasks
        Assert.True(getUserStoryResult.IsSuccess);
        var userStoryModel = getUserStoryResult.Value;
        Assert.NotNull(userStoryModel);
        
        // Verify tasks are excluded
        Assert.Empty(userStoryModel.Tasks);
    }
    
    [Fact]
    public async Task GetUserStoryById_Returns_Correct_Model_After_UserStory_Update()
    {
        // Arrange - Create project and user story
        var projectName = "GUSB Test Project 4";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var originalName = "Original GUSB Story Name";
        var originalDescription = "Original GUSB description";
        var createUserStoryRequest = new CreateUserStoryRequest(originalName, projectId, originalDescription);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Update the user story
        var newName = "Updated GUSB Story Name";
        var newDescription = "Updated GUSB description";
        var updateUserStoryRequest = new UpdateUserStoryRequest(userStoryId, newName, newDescription);
        var updateUserStoryResult = await _updateUserStoryHandler.Handle(updateUserStoryRequest, CancellationToken.None);
        Assert.True(updateUserStoryResult.IsSuccess);
        
        // Act - Get the updated user story by ID
        var getUserStoryRequest = new GetUserStoryByIdRequest(userStoryId);
        var getUserStoryResult = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify the user story model reflects the updates
        Assert.True(getUserStoryResult.IsSuccess);
        var userStoryModel = getUserStoryResult.Value;
        Assert.NotNull(userStoryModel);
        Assert.Equal(newName, userStoryModel.Name);
        Assert.Equal(newDescription, userStoryModel.Description);
    }
    
    [Fact]
    public async Task GetUserStoryById_Returns_UserStoryModel_With_Correct_Structure()
    {
        // Arrange - Create project and user story
        var projectName = "GUSB Test Project 5";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "GUSB Test Story 5";
        var userStoryDescription = "User story for model structure testing";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId, userStoryDescription);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Add a task to ensure the Tasks collection works
        var taskName = "GUSB Task 5";
        var createTaskRequest = new CreateTaskRequest(taskName, "Prompt 5", userStoryId);
        var taskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(taskResult.IsSuccess);
        
        // Act - Get the user story by ID
        var getUserStoryRequest = new GetUserStoryByIdRequest(userStoryId);
        var getUserStoryResult = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify the user story model has the correct structure
        Assert.True(getUserStoryResult.IsSuccess);
        var userStoryModel = getUserStoryResult.Value;
        
        // Check that the model is of type UserStoryModel
        Assert.IsType<UserStoryModel>(userStoryModel);
        
        // Verify the model has all required properties
        Assert.NotEqual(Guid.Empty, userStoryModel.Id);
        Assert.NotEmpty(userStoryModel.Name);
        Assert.NotNull(userStoryModel.Description);
        Assert.NotEqual(Guid.Empty, userStoryModel.ProjectId);
        Assert.NotNull(userStoryModel.Tasks);
        Assert.Single(userStoryModel.Tasks); // Should have one task
        
        // Verify the model's property structure
        var userStoryModelType = typeof(UserStoryModel);
        var properties = userStoryModelType.GetProperties();
        
        // Should only contain the 5 expected properties
        Assert.Equal(5, properties.Length);
        
        // Check that there are no navigation properties to Project entity
        var propertyNames = properties.Select(p => p.Name).ToList();
        Assert.Contains("Id", propertyNames);
        Assert.Contains("Name", propertyNames);
        Assert.Contains("Description", propertyNames);
        Assert.Contains("ProjectId", propertyNames);
        Assert.Contains("Tasks", propertyNames);
        
        // Ensure there are no "Project" navigation properties (only ProjectId)
        Assert.DoesNotContain("Project", propertyNames);
        
        // Check task property type is correct
        var tasksProperty = userStoryModelType.GetProperty("Tasks");
        Assert.Equal(typeof(List<TaskModel>), tasksProperty?.PropertyType);
    }
    
    [Fact]
    public async Task GetUserStoryById_Returns_Empty_Tasks_Collection_For_New_UserStory()
    {
        // Arrange - Create project and user story without tasks
        var projectName = "GUSB Test Project 6";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "GUSB Test Story 6";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Act - Get the user story by ID
        var getUserStoryRequest = new GetUserStoryByIdRequest(userStoryId, excludeTasks: false);
        var getUserStoryResult = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify the user story model has an empty tasks collection, not null
        Assert.True(getUserStoryResult.IsSuccess);
        var userStoryModel = getUserStoryResult.Value;
        Assert.NotNull(userStoryModel);
        Assert.NotNull(userStoryModel.Tasks); // Should never be null
        Assert.Empty(userStoryModel.Tasks);   // Should be empty
    }
}