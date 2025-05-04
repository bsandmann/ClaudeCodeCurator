using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task CreateTask_Succeeds_For_Default_Case()
    {
        // Arrange - Create a project and user story to attach the task to
        var projectName = "Test Project for Task";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "Test User Story for Task";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task
        var taskName = "Test Task";
        var promptBody = "This is a test prompt for the task";
        var taskType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId, taskType);
        
        // Act
        var result = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        var taskId = result.Value;
        Assert.NotEqual(Guid.Empty, taskId);
        
        // Verify the task was created in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var task = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.NotNull(task);
            Assert.Equal(taskName, task.Name);
            Assert.Equal(promptBody, task.PromptBody);
            Assert.Equal(userStoryId, task.UserStoryId);
            Assert.Equal(taskType, task.Type);
        }
    }
    
    [Fact]
    public async Task CreateTask_With_Bug_Type_Succeeds()
    {
        // Arrange - Create a project and user story to attach the task to
        var projectName = "Test Project for Bug Task 2";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "Test User Story for Bug Task 2";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task with Bug type
        var taskName = "Test Bug 2";
        var promptBody = "This is a test bug report";
        var taskType = TaskType.Bug;
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId, taskType);
        
        // Act
        var result = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        var taskId = result.Value;
        
        // Verify the task was created in the database with Bug type
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var task = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.NotNull(task);
            Assert.Equal(TaskType.Bug, task.Type);
        }
    }
    
    [Fact]
    public async Task CreateTask_Fails_For_NonExistent_UserStory()
    {
        // Arrange
        var nonExistentUserStoryId = Guid.NewGuid();
        var createTaskRequest = new CreateTaskRequest(
            "Task with Invalid UserStory", 
            "Prompt body",
            nonExistentUserStoryId);
        
        // Act
        var result = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"User story with ID '{nonExistentUserStoryId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task CreateTask_Fails_For_Duplicate_Name_In_Same_UserStory()
    {
        // Arrange - Create a project and user story
        var projectName = "Project for Duplicate Task Test";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Duplicate Task Test";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task
        var taskName = "Duplicate Task";
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            "First prompt body", 
            userStoryId);
        
        // First creation should succeed
        var firstResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);
        
        // Act - Try to create a second task with the same name in the same user story
        var secondRequest = new CreateTaskRequest(
            taskName, 
            "Different prompt body", 
            userStoryId);
            
        var secondResult = await _createTaskHandler.Handle(secondRequest, CancellationToken.None);
        
        // Assert
        Assert.False(secondResult.IsSuccess);
        Assert.Contains($"Task with name '{taskName}' already exists", 
            secondResult.Errors.First().Message);
    }
    
    [Fact]
    public async Task CreateTask_Succeeds_With_Same_Name_In_Different_UserStories()
    {
        // Arrange - Create a project
        var projectName = "Project for Same Task Name Test";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create two different user stories in the same project
        var userStory1Name = "First User Story";
        var userStory2Name = "Second User Story";
        
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, projectId);
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, projectId);
        
        var userStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        var userStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        
        Assert.True(userStory1Result.IsSuccess);
        Assert.True(userStory2Result.IsSuccess);
        
        var userStory1Id = userStory1Result.Value;
        var userStory2Id = userStory2Result.Value;
        
        // Create a task with the same name in both user stories
        var taskName = "Common Task Name";
        var createTask1Request = new CreateTaskRequest(
            taskName, 
            "First prompt body", 
            userStory1Id);
            
        var createTask2Request = new CreateTaskRequest(
            taskName, 
            "Second prompt body", 
            userStory2Id);
        
        // Act
        var result1 = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        var result2 = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        // Verify both tasks were created
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var task1 = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == result1.Value);
                
            var task2 = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == result2.Value);
            
            Assert.NotNull(task1);
            Assert.NotNull(task2);
            Assert.Equal(taskName, task1.Name);
            Assert.Equal(taskName, task2.Name);
            Assert.Equal(userStory1Id, task1.UserStoryId);
            Assert.Equal(userStory2Id, task2.UserStoryId);
        }
    }
}