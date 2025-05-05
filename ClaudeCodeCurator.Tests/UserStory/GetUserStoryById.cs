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
        
        // Should only contain the 7 expected properties (6 original + CreatedOrUpdatedUtc)
        Assert.Equal(7, properties.Length);
        
        // Check that there are no navigation properties to Project entity
        var propertyNames = properties.Select(p => p.Name).ToList();
        Assert.Contains("Id", propertyNames);
        Assert.Contains("Name", propertyNames);
        Assert.Contains("Description", propertyNames);
        Assert.Contains("UserStoryNumber", propertyNames);
        Assert.Contains("CreatedOrUpdatedUtc", propertyNames);
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
    
    [Fact]
    public async Task UserStoryNumber_Should_Increment_For_Each_New_UserStory()
    {
        // Arrange - Create a project for testing
        var projectName = "UserStoryNumber Increment Test Project";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create first user story
        var userStory1Name = "UserStoryNumber Test Story 1";
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, projectId);
        var createUserStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        
        Assert.True(createUserStory1Result.IsSuccess);
        var userStory1Id = createUserStory1Result.Value;
        
        // Create second user story
        var userStory2Name = "UserStoryNumber Test Story 2";
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, projectId);
        var createUserStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        
        Assert.True(createUserStory2Result.IsSuccess);
        var userStory2Id = createUserStory2Result.Value;
        
        // Create third user story
        var userStory3Name = "UserStoryNumber Test Story 3";
        var createUserStory3Request = new CreateUserStoryRequest(userStory3Name, projectId);
        var createUserStory3Result = await _createUserStoryHandler.Handle(createUserStory3Request, CancellationToken.None);
        
        Assert.True(createUserStory3Result.IsSuccess);
        var userStory3Id = createUserStory3Result.Value;
        
        // Act - Get all three user stories
        var getUserStory1Request = new GetUserStoryByIdRequest(userStory1Id);
        var getUserStory2Request = new GetUserStoryByIdRequest(userStory2Id);
        var getUserStory3Request = new GetUserStoryByIdRequest(userStory3Id);
        
        var getUserStory1Result = await _getUserStoryByIdHandler.Handle(getUserStory1Request, CancellationToken.None);
        var getUserStory2Result = await _getUserStoryByIdHandler.Handle(getUserStory2Request, CancellationToken.None);
        var getUserStory3Result = await _getUserStoryByIdHandler.Handle(getUserStory3Request, CancellationToken.None);
        
        // Assert - Verify all results are successful
        Assert.True(getUserStory1Result.IsSuccess);
        Assert.True(getUserStory2Result.IsSuccess);
        Assert.True(getUserStory3Result.IsSuccess);
        
        var userStoryModel1 = getUserStory1Result.Value;
        var userStoryModel2 = getUserStory2Result.Value;
        var userStoryModel3 = getUserStory3Result.Value;
        
        // Verify user story numbers are sequential and start at 1
        Assert.Equal(1, userStoryModel1.UserStoryNumber);
        Assert.Equal(2, userStoryModel2.UserStoryNumber);
        Assert.Equal(3, userStoryModel3.UserStoryNumber);
    }
    
    [Fact]
    public async Task UserStoryNumber_Should_Be_Project_Specific()
    {
        // Arrange - Create two different projects
        var project1Name = "UserStoryNumber Project 1";
        var project2Name = "UserStoryNumber Project 2";
        
        var createProject1Request = new CreateProjectRequest(project1Name);
        var createProject2Request = new CreateProjectRequest(project2Name);
        
        var createProject1Result = await _createProjectHandler.Handle(createProject1Request, CancellationToken.None);
        var createProject2Result = await _createProjectHandler.Handle(createProject2Request, CancellationToken.None);
        
        Assert.True(createProject1Result.IsSuccess);
        Assert.True(createProject2Result.IsSuccess);
        
        var project1Id = createProject1Result.Value;
        var project2Id = createProject2Result.Value;
        
        // Create user stories for the first project
        var project1UserStory1Name = "Project1 UserStory 1";
        var project1UserStory2Name = "Project1 UserStory 2";
        
        var createProject1UserStory1Request = new CreateUserStoryRequest(project1UserStory1Name, project1Id);
        var createProject1UserStory2Request = new CreateUserStoryRequest(project1UserStory2Name, project1Id);
        
        var createProject1UserStory1Result = await _createUserStoryHandler.Handle(createProject1UserStory1Request, CancellationToken.None);
        var createProject1UserStory2Result = await _createUserStoryHandler.Handle(createProject1UserStory2Request, CancellationToken.None);
        
        Assert.True(createProject1UserStory1Result.IsSuccess);
        Assert.True(createProject1UserStory2Result.IsSuccess);
        
        var project1UserStory1Id = createProject1UserStory1Result.Value;
        var project1UserStory2Id = createProject1UserStory2Result.Value;
        
        // Create user stories for the second project
        var project2UserStory1Name = "Project2 UserStory 1";
        var project2UserStory2Name = "Project2 UserStory 2";
        
        var createProject2UserStory1Request = new CreateUserStoryRequest(project2UserStory1Name, project2Id);
        var createProject2UserStory2Request = new CreateUserStoryRequest(project2UserStory2Name, project2Id);
        
        var createProject2UserStory1Result = await _createUserStoryHandler.Handle(createProject2UserStory1Request, CancellationToken.None);
        var createProject2UserStory2Result = await _createUserStoryHandler.Handle(createProject2UserStory2Request, CancellationToken.None);
        
        Assert.True(createProject2UserStory1Result.IsSuccess);
        Assert.True(createProject2UserStory2Result.IsSuccess);
        
        var project2UserStory1Id = createProject2UserStory1Result.Value;
        var project2UserStory2Id = createProject2UserStory2Result.Value;
        
        // Act - Get all user stories
        var getProject1UserStory1Request = new GetUserStoryByIdRequest(project1UserStory1Id);
        var getProject1UserStory2Request = new GetUserStoryByIdRequest(project1UserStory2Id);
        var getProject2UserStory1Request = new GetUserStoryByIdRequest(project2UserStory1Id);
        var getProject2UserStory2Request = new GetUserStoryByIdRequest(project2UserStory2Id);
        
        var getProject1UserStory1Result = await _getUserStoryByIdHandler.Handle(getProject1UserStory1Request, CancellationToken.None);
        var getProject1UserStory2Result = await _getUserStoryByIdHandler.Handle(getProject1UserStory2Request, CancellationToken.None);
        var getProject2UserStory1Result = await _getUserStoryByIdHandler.Handle(getProject2UserStory1Request, CancellationToken.None);
        var getProject2UserStory2Result = await _getUserStoryByIdHandler.Handle(getProject2UserStory2Request, CancellationToken.None);
        
        // Assert - Verify all results are successful
        Assert.True(getProject1UserStory1Result.IsSuccess);
        Assert.True(getProject1UserStory2Result.IsSuccess);
        Assert.True(getProject2UserStory1Result.IsSuccess);
        Assert.True(getProject2UserStory2Result.IsSuccess);
        
        var project1UserStory1Model = getProject1UserStory1Result.Value;
        var project1UserStory2Model = getProject1UserStory2Result.Value;
        var project2UserStory1Model = getProject2UserStory1Result.Value;
        var project2UserStory2Model = getProject2UserStory2Result.Value;
        
        // Verify user story numbers are sequential within each project and both start at 1
        Assert.Equal(1, project1UserStory1Model.UserStoryNumber);
        Assert.Equal(2, project1UserStory2Model.UserStoryNumber);
        Assert.Equal(1, project2UserStory1Model.UserStoryNumber);
        Assert.Equal(2, project2UserStory2Model.UserStoryNumber);
    }
}