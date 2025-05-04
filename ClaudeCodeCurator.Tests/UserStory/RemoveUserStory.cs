using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetUserStoryById;
using ClaudeCodeCurator.Commands.RemoveUserStory;
using ClaudeCodeCurator.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task RemoveUserStory_Succeeds_For_Existing_UserStory()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveUserStory Test Project 1";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveUserStory Test Story 1";
        var userStoryDescription = "User story to be removed";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId, userStoryDescription);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Verify the user story exists before removal
        var getUserStoryRequest = new GetUserStoryByIdRequest(userStoryId);
        var getUserStoryResult = await _getUserStoryByIdHandler.Handle(getUserStoryRequest, CancellationToken.None);
        Assert.True(getUserStoryResult.IsSuccess);
        
        // Act - Remove the user story
        var removeUserStoryRequest = new RemoveUserStoryRequest(userStoryId);
        var removeUserStoryResult = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeUserStoryResult.IsSuccess);
        Assert.True(removeUserStoryResult.Value);
        
        // Verify the user story no longer exists in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.Null(userStory);
        }
    }
    
    [Fact]
    public async Task RemoveUserStory_Fails_For_NonExistent_UserStory()
    {
        // Arrange - Use a non-existent user story ID
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var removeUserStoryRequest = new RemoveUserStoryRequest(nonExistentId);
        var result = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"User story with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task RemoveUserStory_With_CascadeTasks_True_Removes_Tasks()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveUserStory Test Project 2";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveUserStory Test Story 2";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create tasks for this user story
        var taskIds = new List<Guid>();
        for (int i = 1; i <= 3; i++)
        {
            var createTaskRequest = new CreateTaskRequest(
                $"RemoveUserStory Test Task {i}", 
                $"Task {i} to be removed with user story", 
                userStoryId);
                
            var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
            Assert.True(createTaskResult.IsSuccess);
            taskIds.Add(createTaskResult.Value);
        }
        
        // Verify tasks exist before removal
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var tasksCount = await context.Tasks
                .AsNoTracking()
                .Where(t => t.UserStoryId == userStoryId)
                .CountAsync();
                
            Assert.Equal(3, tasksCount);
        }
        
        // Act - Remove the user story with cascadeTasks=true (default)
        var removeUserStoryRequest = new RemoveUserStoryRequest(userStoryId, cascadeTasks: true);
        var removeUserStoryResult = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeUserStoryResult.IsSuccess);
        
        // Verify the user story and its tasks no longer exist
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Check user story is gone
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.Null(userStory);
            
            // Check all tasks are gone
            var tasksCount = await context.Tasks
                .AsNoTracking()
                .Where(t => t.UserStoryId == userStoryId)
                .CountAsync();
                
            Assert.Equal(0, tasksCount);
            
            // Verify each task ID specifically
            foreach (var taskId in taskIds)
            {
                var task = await context.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == taskId);
                    
                Assert.Null(task);
            }
        }
    }
    
    [Fact]
    public async Task RemoveUserStory_With_CascadeTasks_False_Fails_When_Tasks_Exist()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveUserStory Test Project 3";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveUserStory Test Story 3";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task for this user story
        var createTaskRequest = new CreateTaskRequest(
            "RemoveUserStory Test Task 4", 
            "Task that should prevent user story removal", 
            userStoryId);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Try to remove the user story with cascadeTasks=false
        var removeUserStoryRequest = new RemoveUserStoryRequest(userStoryId, cascadeTasks: false);
        var removeUserStoryResult = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify removal fails
        Assert.False(removeUserStoryResult.IsSuccess);
        Assert.Contains("has associated tasks", removeUserStoryResult.Errors.First().Message);
        
        // Verify the user story and task still exist
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Check user story still exists
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.NotNull(userStory);
            
            // Check task still exists
            var task = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.NotNull(task);
        }
    }
    
    [Fact]
    public async Task RemoveUserStory_With_CascadeTasks_False_Succeeds_When_No_Tasks_Exist()
    {
        // Arrange - Create a project and user story without tasks
        var projectName = "RemoveUserStory Test Project 4";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveUserStory Test Story 4";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Act - Remove the user story with cascadeTasks=false
        var removeUserStoryRequest = new RemoveUserStoryRequest(userStoryId, cascadeTasks: false);
        var removeUserStoryResult = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeUserStoryResult.IsSuccess);
        
        // Verify the user story no longer exists
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.Null(userStory);
        }
    }
    
    [Fact]
    public async Task RemoveUserStory_Does_Not_Affect_Other_UserStories()
    {
        // Arrange - Create a project and multiple user stories
        var projectName = "RemoveUserStory Test Project 5";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create two user stories
        var userStory1Name = "RemoveUserStory Test Story 5";
        var userStory2Name = "RemoveUserStory Test Story 6";
        
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, projectId);
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, projectId);
        
        var userStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        var userStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        
        Assert.True(userStory1Result.IsSuccess);
        Assert.True(userStory2Result.IsSuccess);
        
        var userStory1Id = userStory1Result.Value;
        var userStory2Id = userStory2Result.Value;
        
        // Act - Remove only the first user story
        var removeUserStoryRequest = new RemoveUserStoryRequest(userStory1Id);
        var removeUserStoryResult = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeUserStoryResult.IsSuccess);
        
        // Verify the first user story no longer exists
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // First user story should be gone
            var userStory1 = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStory1Id);
                
            Assert.Null(userStory1);
            
            // Second user story should still exist
            var userStory2 = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStory2Id);
                
            Assert.NotNull(userStory2);
            Assert.Equal(userStory2Name, userStory2.Name);
        }
    }
    
    [Fact]
    public async Task RemoveUserStory_Does_Not_Affect_Parent_Project()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveUserStory Test Project 6";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "RemoveUserStory Test Story 7";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Act - Remove the user story
        var removeUserStoryRequest = new RemoveUserStoryRequest(userStoryId);
        var removeUserStoryResult = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeUserStoryResult.IsSuccess);
        
        // Verify the user story no longer exists but project still exists
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // User story should be gone
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.Null(userStory);
            
            // Project should still exist
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            Assert.NotNull(project);
            Assert.Equal(projectName, project.Name);
        }
    }
    
    [Fact]
    public async Task RemoveUserStory_Can_Remove_All_UserStories_From_Project()
    {
        // Arrange - Create a project and multiple user stories
        var projectName = "RemoveUserStory Test Project 7";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create multiple user stories
        var userStoryIds = new List<Guid>();
        for (int i = 1; i <= 3; i++)
        {
            var createUserStoryRequest = new CreateUserStoryRequest(
                $"RemoveUserStory Test Multiple Story {i}", 
                projectId);
                
            var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
            Assert.True(createUserStoryResult.IsSuccess);
            userStoryIds.Add(createUserStoryResult.Value);
        }
        
        // Act - Remove all user stories one by one
        foreach (var userStoryId in userStoryIds)
        {
            var removeUserStoryRequest = new RemoveUserStoryRequest(userStoryId);
            var removeUserStoryResult = await _removeUserStoryHandler.Handle(removeUserStoryRequest, CancellationToken.None);
            Assert.True(removeUserStoryResult.IsSuccess);
        }
        
        // Assert - Verify all user stories were removed
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Verify no user stories exist for this project
            var userStoriesCount = await context.UserStories
                .AsNoTracking()
                .Where(us => us.ProjectId == projectId)
                .CountAsync();
                
            Assert.Equal(0, userStoriesCount);
            
            // Verify the project still exists
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            Assert.NotNull(project);
        }
    }
}