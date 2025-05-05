using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetProjectByLastUsed;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task GetProjectByLastUsed_Returns_Failure_When_No_Tasks_Exist()
    {
        // Arrange - Ensure clean database
        await CleanupProjects();
        
        // Create a project without any user stories or tasks
        var projectName = "GPLU Test Project 1";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        Assert.True(createProjectResult.IsSuccess);
        
        // Act - Attempt to get project by last used
        var getProjectByLastUsedRequest = new GetProjectByLastUsedRequest();
        var getProjectByLastUsedResult = await _getProjectByLastUsedHandler.Handle(getProjectByLastUsedRequest, CancellationToken.None);
        
        // Assert - Should return failure since no tasks exist
        Assert.False(getProjectByLastUsedResult.IsSuccess);
        Assert.Contains("No tasks found", getProjectByLastUsedResult.Errors.First().Message);
    }
    
    [Fact]
    public async Task GetProjectByLastUsed_Returns_Project_With_Most_Recently_Updated_Task()
    {
        // Arrange - Ensure clean database
        await CleanupProjects();
        
        // Create two projects with user stories and tasks
        // Project 1
        var project1Name = "GPLU Test Project 2";
        var createProject1Request = new CreateProjectRequest(project1Name);
        var createProject1Result = await _createProjectHandler.Handle(createProject1Request, CancellationToken.None);
        Assert.True(createProject1Result.IsSuccess);
        var project1Id = createProject1Result.Value;
        
        // Project 2
        var project2Name = "GPLU Test Project 3";
        var createProject2Request = new CreateProjectRequest(project2Name);
        var createProject2Result = await _createProjectHandler.Handle(createProject2Request, CancellationToken.None);
        Assert.True(createProject2Result.IsSuccess);
        var project2Id = createProject2Result.Value;
        
        // Create user stories for both projects
        var userStory1Name = "GPLU User Story 1";
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, project1Id);
        var createUserStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        Assert.True(createUserStory1Result.IsSuccess);
        var userStory1Id = createUserStory1Result.Value;
        
        var userStory2Name = "GPLU User Story 2";
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, project2Id);
        var createUserStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        Assert.True(createUserStory2Result.IsSuccess);
        var userStory2Id = createUserStory2Result.Value;
        
        // Create tasks for both user stories
        // Task for Project 1
        var task1Name = "GPLU Task 1";
        var createTask1Request = new CreateTaskRequest(task1Name, "Task for Project 1", userStory1Id);
        var createTask1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        Assert.True(createTask1Result.IsSuccess);
        
        // Wait a moment to ensure different timestamps
        await Task.Delay(10);
        
        // Task for Project 2 - created later (most recent)
        var task2Name = "GPLU Task 2";
        var createTask2Request = new CreateTaskRequest(task2Name, "Task for Project 2", userStory2Id);
        var createTask2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        Assert.True(createTask2Result.IsSuccess);
        
        // Act - Get project by last used task
        var getProjectByLastUsedRequest = new GetProjectByLastUsedRequest();
        var getProjectByLastUsedResult = await _getProjectByLastUsedHandler.Handle(getProjectByLastUsedRequest, CancellationToken.None);
        
        // Assert - Should return Project 2 as it has the most recently updated task
        Assert.True(getProjectByLastUsedResult.IsSuccess);
        var projectModel = getProjectByLastUsedResult.Value;
        Assert.NotNull(projectModel);
        Assert.Equal(project2Id, projectModel.Id);
        Assert.Equal(project2Name, projectModel.Name);
        
        // Verify user stories and tasks are loaded
        Assert.Single(projectModel.UserStories);
        var userStory = projectModel.UserStories.First();
        Assert.Equal(userStory2Name, userStory.Name);
        Assert.Single(userStory.Tasks);
        Assert.Equal(task2Name, userStory.Tasks.First().Name);
    }
    
    [Fact]
    public async Task GetProjectByLastUsed_Returns_Project_With_Most_Recently_Updated_Task_After_Update()
    {
        // Arrange - Ensure clean database
        await CleanupProjects();
        
        // Create two projects with user stories and tasks
        // Project 1
        var project1Name = "GPLU Test Project 4";
        var createProject1Request = new CreateProjectRequest(project1Name);
        var createProject1Result = await _createProjectHandler.Handle(createProject1Request, CancellationToken.None);
        Assert.True(createProject1Result.IsSuccess);
        var project1Id = createProject1Result.Value;
        
        // Project 2
        var project2Name = "GPLU Test Project 5";
        var createProject2Request = new CreateProjectRequest(project2Name);
        var createProject2Result = await _createProjectHandler.Handle(createProject2Request, CancellationToken.None);
        Assert.True(createProject2Result.IsSuccess);
        var project2Id = createProject2Result.Value;
        
        // Create user stories for both projects
        var userStory1Name = "GPLU User Story 3";
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, project1Id);
        var createUserStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        Assert.True(createUserStory1Result.IsSuccess);
        var userStory1Id = createUserStory1Result.Value;
        
        var userStory2Name = "GPLU User Story 4";
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, project2Id);
        var createUserStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        Assert.True(createUserStory2Result.IsSuccess);
        var userStory2Id = createUserStory2Result.Value;
        
        // Create tasks for both user stories
        // Task for Project 2 (created first)
        var task2Name = "GPLU Task 3";
        var createTask2Request = new CreateTaskRequest(task2Name, "Task for Project 2", userStory2Id);
        var createTask2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        Assert.True(createTask2Result.IsSuccess);
        var task2Id = createTask2Result.Value;
        
        // Wait a moment to ensure different timestamps
        await Task.Delay(10);
        
        // Task for Project 1 (created second - most recent initially)
        var task1Name = "GPLU Task 4";
        var createTask1Request = new CreateTaskRequest(task1Name, "Task for Project 1", userStory1Id);
        var createTask1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        Assert.True(createTask1Result.IsSuccess);
        var task1Id = createTask1Result.Value;
        
        // Verify that Project 1 is returned as last used initially
        var getProject1Request = new GetProjectByLastUsedRequest();
        var getProject1Result = await _getProjectByLastUsedHandler.Handle(getProject1Request, CancellationToken.None);
        Assert.True(getProject1Result.IsSuccess);
        Assert.Equal(project1Id, getProject1Result.Value.Id);
        
        // Update task in Project 2 to make it the most recently updated
        await Task.Delay(10);
        using var scope = _serviceScopeFactoryMock.Object.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        var task2Entity = await context.Tasks.FindAsync(task2Id);
        Assert.NotNull(task2Entity);
        
        task2Entity.Name = "GPLU Task 3 - Updated";
        task2Entity.CreatedOrUpdatedUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        
        // Act - Get project by last used task
        var getProject2Request = new GetProjectByLastUsedRequest();
        var getProject2Result = await _getProjectByLastUsedHandler.Handle(getProject2Request, CancellationToken.None);
        
        // Assert - Should return Project 2 as it has the most recently updated task
        Assert.True(getProject2Result.IsSuccess);
        var projectModel = getProject2Result.Value;
        Assert.NotNull(projectModel);
        Assert.Equal(project2Id, projectModel.Id);
        Assert.Equal(project2Name, projectModel.Name);
    }
    
    [Fact]
    public async Task GetProjectByLastUsed_Returns_Complete_Project_Model_With_All_UserStories_And_Tasks()
    {
        // Arrange - Ensure clean database
        await CleanupProjects();
        
        // Create a project with multiple user stories and tasks
        var projectName = "GPLU Test Project 6";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create multiple user stories
        var userStory1Name = "GPLU User Story 5";
        var userStory2Name = "GPLU User Story 6";
        
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, projectId);
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, projectId);
        
        var createUserStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        var createUserStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        
        Assert.True(createUserStory1Result.IsSuccess);
        Assert.True(createUserStory2Result.IsSuccess);
        
        var userStory1Id = createUserStory1Result.Value;
        var userStory2Id = createUserStory2Result.Value;
        
        // Create multiple tasks
        var task1Name = "GPLU Task 5";
        var task2Name = "GPLU Task 6";
        var task3Name = "GPLU Task 7";
        
        var createTask1Request = new CreateTaskRequest(task1Name, "Task 1", userStory1Id);
        var createTask2Request = new CreateTaskRequest(task2Name, "Task 2", userStory1Id);
        var createTask3Request = new CreateTaskRequest(task3Name, "Task 3", userStory2Id);
        
        var createTask1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        var createTask2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        
        // Delay to ensure this is the most recent task
        await Task.Delay(10);
        var createTask3Result = await _createTaskHandler.Handle(createTask3Request, CancellationToken.None);
        
        Assert.True(createTask1Result.IsSuccess);
        Assert.True(createTask2Result.IsSuccess);
        Assert.True(createTask3Result.IsSuccess);
        
        // Act - Get project by last used
        var getProjectByLastUsedRequest = new GetProjectByLastUsedRequest();
        var getProjectByLastUsedResult = await _getProjectByLastUsedHandler.Handle(getProjectByLastUsedRequest, CancellationToken.None);
        
        // Assert - Should return the complete project with all user stories and tasks
        Assert.True(getProjectByLastUsedResult.IsSuccess);
        var projectModel = getProjectByLastUsedResult.Value;
        Assert.NotNull(projectModel);
        Assert.Equal(projectId, projectModel.Id);
        Assert.Equal(projectName, projectModel.Name);
        
        // Verify all user stories are included
        Assert.Equal(2, projectModel.UserStories.Count);
        
        // Check that user stories have correct names
        var userStoryNames = projectModel.UserStories.Select(us => us.Name).ToList();
        Assert.Contains(userStory1Name, userStoryNames);
        Assert.Contains(userStory2Name, userStoryNames);
        
        // Find both user stories
        var userStory1 = projectModel.UserStories.FirstOrDefault(us => us.Name == userStory1Name);
        var userStory2 = projectModel.UserStories.FirstOrDefault(us => us.Name == userStory2Name);
        
        Assert.NotNull(userStory1);
        Assert.NotNull(userStory2);
        
        // Verify first user story has 2 tasks
        Assert.Equal(2, userStory1.Tasks.Count);
        
        // Verify second user story has 1 task
        Assert.Single(userStory2.Tasks);
        
        // Verify task names
        var userStory1TaskNames = userStory1.Tasks.Select(t => t.Name).ToList();
        Assert.Contains(task1Name, userStory1TaskNames);
        Assert.Contains(task2Name, userStory1TaskNames);
        
        Assert.Equal(task3Name, userStory2.Tasks.First().Name);
    }
    
    // CleanupProjects method moved to IntegrationTests parent class
}