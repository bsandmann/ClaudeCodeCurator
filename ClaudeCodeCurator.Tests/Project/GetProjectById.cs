using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetProjectById;
using ClaudeCodeCurator.Commands.UpdateProject;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.Models;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task GetProjectById_Returns_Project_When_Exists()
    {
        // Arrange - Create a project
        var projectName = "GPB Test Project 1";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Act - Get the project by ID
        var getProjectRequest = new GetProjectByIdRequest(projectId);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success
        Assert.True(getProjectResult.IsSuccess);
        
        // Verify the project model has the correct properties
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        Assert.Equal(projectId, projectModel.Id);
        Assert.Equal(projectName, projectModel.Name);
        Assert.Empty(projectModel.UserStories); // Should be empty as we haven't created any user stories
    }
    
    [Fact]
    public async Task GetProjectById_Returns_Failure_When_Project_Not_Found()
    {
        // Arrange - Use a non-existent project ID
        var nonExistentId = Guid.NewGuid();
        var getProjectRequest = new GetProjectByIdRequest(nonExistentId);
        
        // Act
        var result = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Project with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task GetProjectById_Returns_Project_With_UserStories_When_Included()
    {
        // Arrange - Create a project
        var projectName = "GPB Test Project 2";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create multiple user stories for this project
        var userStory1Name = "GPB User Story 1";
        var userStory2Name = "GPB User Story 2";
        var userStory3Name = "GPB User Story 3";
        
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, projectId, "Description 1");
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, projectId, "Description 2");
        var createUserStory3Request = new CreateUserStoryRequest(userStory3Name, projectId, "Description 3");
        
        var userStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        var userStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        var userStory3Result = await _createUserStoryHandler.Handle(createUserStory3Request, CancellationToken.None);
        
        Assert.True(userStory1Result.IsSuccess);
        Assert.True(userStory2Result.IsSuccess);
        Assert.True(userStory3Result.IsSuccess);
        
        // Act - Get the project by ID with user stories included (default behavior)
        var getProjectRequest = new GetProjectByIdRequest(projectId, excludeUserStories: false);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success with user stories
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        
        // Verify user stories are included and have the correct properties
        Assert.Equal(3, projectModel.UserStories.Count);
        
        // Verify user stories have the expected names
        var userStoryNames = projectModel.UserStories.Select(us => us.Name).ToList();
        Assert.Contains(userStory1Name, userStoryNames);
        Assert.Contains(userStory2Name, userStoryNames);
        Assert.Contains(userStory3Name, userStoryNames);
        
        // Verify user stories have the correct project ID
        foreach (var userStory in projectModel.UserStories)
        {
            Assert.Equal(projectId, userStory.ProjectId);
        }
    }
    
    [Fact]
    public async Task GetProjectById_Returns_Project_Without_UserStories_When_Excluded()
    {
        // Arrange - Create a project
        var projectName = "GPB Test Project 3";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create a user story for this project
        var userStoryName = "GPB User Story 4";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var userStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(userStoryResult.IsSuccess);
        
        // Act - Get the project by ID with user stories explicitly excluded
        var getProjectRequest = new GetProjectByIdRequest(projectId, excludeUserStories: true);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success without user stories
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        
        // Verify user stories are excluded
        Assert.Empty(projectModel.UserStories);
    }
    
    [Fact]
    public async Task GetProjectById_Returns_Project_With_UserStories_And_Tasks()
    {
        // Arrange - Create a project
        var projectName = "GPB Test Project 4";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create user stories for this project
        var userStory1Name = "GPB User Story 5";
        var userStory2Name = "GPB User Story 6";
        
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, projectId);
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, projectId);
        
        var userStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        var userStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        
        Assert.True(userStory1Result.IsSuccess);
        Assert.True(userStory2Result.IsSuccess);
        
        var userStory1Id = userStory1Result.Value;
        var userStory2Id = userStory2Result.Value;
        
        // Create tasks for each user story
        var task1Name = "GPB Task 1";
        var task2Name = "GPB Task 2";
        var task3Name = "GPB Task 3";
        
        var createTask1Request = new CreateTaskRequest(task1Name, "GPB Prompt 1", userStory1Id, TaskType.Task);
        var createTask2Request = new CreateTaskRequest(task2Name, "GPB Prompt 2", userStory1Id, TaskType.Bug);
        var createTask3Request = new CreateTaskRequest(task3Name, "GPB Prompt 3", userStory2Id, TaskType.Verify);
        
        var task1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        var task2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        var task3Result = await _createTaskHandler.Handle(createTask3Request, CancellationToken.None);
        
        Assert.True(task1Result.IsSuccess);
        Assert.True(task2Result.IsSuccess);
        Assert.True(task3Result.IsSuccess);
        
        // Act - Get the project by ID with user stories and tasks included
        var getProjectRequest = new GetProjectByIdRequest(projectId, excludeUserStories: false, excludeTasks: false);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success with user stories and tasks
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        
        // Verify user stories are included
        Assert.Equal(2, projectModel.UserStories.Count);
        
        // Find both user stories by their names
        var userStory1 = projectModel.UserStories.FirstOrDefault(us => us.Name == userStory1Name);
        var userStory2 = projectModel.UserStories.FirstOrDefault(us => us.Name == userStory2Name);
        
        Assert.NotNull(userStory1);
        Assert.NotNull(userStory2);
        
        // Verify first user story has 2 tasks
        Assert.Equal(2, userStory1.Tasks.Count);
        
        // Verify second user story has 1 task
        Assert.Equal(1, userStory2.Tasks.Count);
        
        // Verify task details
        Assert.Contains(userStory1.Tasks, t => t.Name == task1Name && t.Type == TaskType.Task);
        Assert.Contains(userStory1.Tasks, t => t.Name == task2Name && t.Type == TaskType.Bug);
        Assert.Contains(userStory2.Tasks, t => t.Name == task3Name && t.Type == TaskType.Verify);
    }
    
    [Fact]
    public async Task GetProjectById_Returns_Project_With_UserStories_But_Without_Tasks()
    {
        // Arrange - Create a project
        var projectName = "GPB Test Project 5";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create a user story for this project
        var userStoryName = "GPB User Story 7";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var userStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(userStoryResult.IsSuccess);
        var userStoryId = userStoryResult.Value;
        
        // Create a task for this user story
        var taskName = "GPB Task 4";
        var createTaskRequest = new CreateTaskRequest(taskName, "GPB Prompt 4", userStoryId);
        var taskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(taskResult.IsSuccess);
        
        // Act - Get the project by ID with user stories included but tasks excluded
        var getProjectRequest = new GetProjectByIdRequest(projectId, excludeUserStories: false, excludeTasks: true);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success with user stories but without tasks
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        
        // Verify user stories are included
        Assert.Single(projectModel.UserStories);
        var userStory = projectModel.UserStories.First();
        Assert.Equal(userStoryName, userStory.Name);
        
        // Verify tasks are excluded
        Assert.Empty(userStory.Tasks);
    }
    
    [Fact]
    public async Task GetProjectById_Returns_Correct_Model_After_Project_Update()
    {
        // Arrange - Create a project
        var originalName = "Original GPB Project Name";
        var createProjectRequest = new CreateProjectRequest(originalName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Update the project
        var newName = "Updated GPB Project Name";
        var updateProjectRequest = new UpdateProjectRequest(projectId, newName);
        var updateProjectResult = await _updateProjectHandler.Handle(updateProjectRequest, CancellationToken.None);
        Assert.True(updateProjectResult.IsSuccess);
        
        // Act - Get the updated project by ID
        var getProjectRequest = new GetProjectByIdRequest(projectId);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify the project model reflects the updates
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        Assert.Equal(newName, projectModel.Name);
    }
    
    [Fact]
    public async Task GetProjectById_Returns_ProjectModel_With_Correct_Structure()
    {
        // Arrange - Create a project
        var projectName = "GPB Test Project 6";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Add a user story to ensure the UserStories collection works
        var userStoryName = "GPB User Story 8";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var userStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(userStoryResult.IsSuccess);
        
        // Act - Get the project by ID
        var getProjectRequest = new GetProjectByIdRequest(projectId);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify the project model has the correct structure
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        
        // Check that the model is of type ProjectModel
        Assert.IsType<ProjectModel>(projectModel);
        
        // Verify the model has all required properties
        Assert.NotEqual(Guid.Empty, projectModel.Id);
        Assert.NotEmpty(projectModel.Name);
        Assert.NotNull(projectModel.UserStories);
        Assert.Single(projectModel.UserStories); // Should have one user story
        
        // Verify the model's property structure
        var projectModelType = typeof(ProjectModel);
        var properties = projectModelType.GetProperties();
        
        // Should only contain the 7 expected properties (6 original + CreatedOrUpdatedUtc)
        Assert.Equal(7, properties.Length);
        
        // Check property names
        var propertyNames = properties.Select(p => p.Name).ToList();
        Assert.Contains("Id", propertyNames);
        Assert.Contains("Name", propertyNames);
        Assert.Contains("PrimePrompt", propertyNames);
        Assert.Contains("UserStoryNumberCounter", propertyNames);
        Assert.Contains("TaskNumberCounter", propertyNames);
        Assert.Contains("CreatedOrUpdatedUtc", propertyNames);
        Assert.Contains("UserStories", propertyNames);
        
        // Check user story property type is correct
        var userStoriesProperty = projectModelType.GetProperty("UserStories");
        Assert.Equal(typeof(List<UserStoryModel>), userStoriesProperty?.PropertyType);
    }
    
    [Fact]
    public async Task GetProjectById_Returns_Empty_UserStories_Collection_For_New_Project()
    {
        // Arrange - Create a project without user stories
        var projectName = "GPB Test Project 7";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Act - Get the project by ID
        var getProjectRequest = new GetProjectByIdRequest(projectId, excludeUserStories: false);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify the project model has an empty user stories collection, not null
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        Assert.NotNull(projectModel.UserStories); // Should never be null
        Assert.Empty(projectModel.UserStories);   // Should be empty
    }
    
    [Fact]
    public async Task GetProjectById_ExcludeUserStories_Also_Excludes_Tasks()
    {
        // Arrange - Create a project with user story and task
        var projectName = "GPB Test Project 8";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create a user story
        var userStoryName = "GPB User Story 9";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var userStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(userStoryResult.IsSuccess);
        var userStoryId = userStoryResult.Value;
        
        // Create a task
        var taskName = "GPB Task 5";
        var createTaskRequest = new CreateTaskRequest(taskName, "GPB Prompt 5", userStoryId);
        var taskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(taskResult.IsSuccess);
        
        // Act - Get the project by ID with user stories excluded
        // This should implicitly exclude tasks as well, as per the GetProjectByIdRequest constructor logic
        var getProjectRequest = new GetProjectByIdRequest(projectId, excludeUserStories: true, excludeTasks: false);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success without user stories or tasks
        Assert.True(getProjectResult.IsSuccess);
        var projectModel = getProjectResult.Value;
        Assert.NotNull(projectModel);
        
        // Verify user stories are excluded
        Assert.Empty(projectModel.UserStories);
        
        // Verify that the excludeTasks parameter is true when excludeUserStories is true
        var requestType = typeof(GetProjectByIdRequest);
        var constructorInfo = requestType.GetConstructor(new[] { typeof(Guid), typeof(bool), typeof(bool) });
        var instance = constructorInfo?.Invoke(new object[] { projectId, true, false }) as GetProjectByIdRequest;
        
        Assert.NotNull(instance);
        var excludeTasksProperty = requestType.GetProperty("ExcludeTasks")?.GetValue(instance);
        Assert.True((bool)excludeTasksProperty!);
    }
}