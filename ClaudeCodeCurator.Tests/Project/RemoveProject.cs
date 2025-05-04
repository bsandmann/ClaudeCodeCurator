using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetProjectById;
using ClaudeCodeCurator.Commands.RemoveProject;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task RemoveProject_Succeeds_For_Existing_Project()
    {
        // Arrange - Create a project
        var projectName = "RemoveProject Test Project 1";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Verify the project exists before removal
        var getProjectRequest = new GetProjectByIdRequest(projectId);
        var getProjectResult = await _getProjectByIdHandler.Handle(getProjectRequest, CancellationToken.None);
        Assert.True(getProjectResult.IsSuccess);
        
        // Act - Remove the project
        var removeProjectRequest = new RemoveProjectRequest(projectId);
        var removeProjectResult = await _removeProjectHandler.Handle(removeProjectRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeProjectResult.IsSuccess);
        Assert.True(removeProjectResult.Value);
        
        // Verify the project no longer exists in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            Assert.Null(project);
        }
    }
    
    [Fact]
    public async Task RemoveProject_Fails_For_NonExistent_Project()
    {
        // Arrange - Use a non-existent project ID
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var removeProjectRequest = new RemoveProjectRequest(nonExistentId);
        var result = await _removeProjectHandler.Handle(removeProjectRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Project with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task RemoveProject_With_CascadeDelete_True_Removes_UserStories_And_Tasks()
    {
        // Arrange - Create a project with user stories and tasks
        var projectName = "RemoveProject Test Project 2";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create multiple user stories
        var userStoryIds = new List<Guid>();
        for (int i = 1; i <= 2; i++)
        {
            var createUserStoryRequest = new CreateUserStoryRequest(
                $"RemoveProject Test Story {i}", 
                projectId);
                
            var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
            Assert.True(createUserStoryResult.IsSuccess);
            userStoryIds.Add(createUserStoryResult.Value);
        }
        
        // Create tasks for each user story
        var taskIds = new List<Guid>();
        foreach (var userStoryId in userStoryIds)
        {
            for (int i = 1; i <= 2; i++)
            {
                var createTaskRequest = new CreateTaskRequest(
                    $"RemoveProject Test Task for {userStoryId} {i}", 
                    $"Task prompt {i}", 
                    userStoryId);
                    
                var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
                Assert.True(createTaskResult.IsSuccess);
                taskIds.Add(createTaskResult.Value);
            }
        }
        
        // Verify entities exist before removal
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Verify project exists
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.NotNull(project);
            
            // Verify user stories exist
            var userStoriesCount = await context.UserStories
                .AsNoTracking()
                .Where(us => us.ProjectId == projectId)
                .CountAsync();
            Assert.Equal(2, userStoriesCount);
            
            // Verify tasks exist
            var tasksCount = await context.Tasks
                .AsNoTracking()
                .Where(t => userStoryIds.Contains(t.UserStoryId))
                .CountAsync();
            Assert.Equal(4, tasksCount);
        }
        
        // Act - Remove the project with cascadeDelete=true (default)
        var removeProjectRequest = new RemoveProjectRequest(projectId, cascadeDelete: true);
        var removeProjectResult = await _removeProjectHandler.Handle(removeProjectRequest, CancellationToken.None);
        
        // Print any error messages
        if (!removeProjectResult.IsSuccess && removeProjectResult.Errors.Any())
        {
            Console.WriteLine($"Error: {removeProjectResult.Errors.First().Message}");
        }
        
        // Assert - Verify removal success
        Assert.True(removeProjectResult.IsSuccess);
        
        // Verify the project, user stories, and tasks no longer exist
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Check project is gone
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.Null(project);
            
            // Check all user stories are gone
            var userStoriesCount = await context.UserStories
                .AsNoTracking()
                .Where(us => us.ProjectId == projectId)
                .CountAsync();
            Assert.Equal(0, userStoriesCount);
            
            // Check all tasks are gone
            var tasksCount = await context.Tasks
                .AsNoTracking()
                .Where(t => userStoryIds.Contains(t.UserStoryId))
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
    public async Task RemoveProject_With_CascadeDelete_False_Fails_When_UserStories_Exist()
    {
        // Arrange - Create a project and user story
        var projectName = "RemoveProject Test Project 3";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create a user story for this project
        var userStoryName = "RemoveProject Test Story 3";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Act - Try to remove the project with cascadeDelete=false
        var removeProjectRequest = new RemoveProjectRequest(projectId, cascadeDelete: false);
        var removeProjectResult = await _removeProjectHandler.Handle(removeProjectRequest, CancellationToken.None);
        
        // Assert - Verify removal fails
        Assert.False(removeProjectResult.IsSuccess);
        Assert.Contains("has associated user stories", removeProjectResult.Errors.First().Message);
        
        // Verify the project and user story still exist
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Check project still exists
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.NotNull(project);
            
            // Check user story still exists
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
            Assert.NotNull(userStory);
        }
    }
    
    [Fact]
    public async Task RemoveProject_With_CascadeDelete_False_Succeeds_When_No_UserStories_Exist()
    {
        // Arrange - Create a project without user stories
        var projectName = "RemoveProject Test Project 4";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Act - Remove the project with cascadeDelete=false
        var removeProjectRequest = new RemoveProjectRequest(projectId, cascadeDelete: false);
        var removeProjectResult = await _removeProjectHandler.Handle(removeProjectRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeProjectResult.IsSuccess);
        
        // Verify the project no longer exists
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.Null(project);
        }
    }
    
    [Fact]
    public async Task RemoveProject_Does_Not_Affect_Other_Projects()
    {
        // Arrange - Create multiple projects
        var project1Name = "RemoveProject Test Project 5";
        var project2Name = "RemoveProject Test Project 6";
        
        var createProject1Request = new CreateProjectRequest(project1Name);
        var createProject2Request = new CreateProjectRequest(project2Name);
        
        var project1Result = await _createProjectHandler.Handle(createProject1Request, CancellationToken.None);
        var project2Result = await _createProjectHandler.Handle(createProject2Request, CancellationToken.None);
        
        Assert.True(project1Result.IsSuccess);
        Assert.True(project2Result.IsSuccess);
        
        var project1Id = project1Result.Value;
        var project2Id = project2Result.Value;
        
        // Act - Remove only the first project
        var removeProjectRequest = new RemoveProjectRequest(project1Id);
        var removeProjectResult = await _removeProjectHandler.Handle(removeProjectRequest, CancellationToken.None);
        
        // Assert - Verify removal success
        Assert.True(removeProjectResult.IsSuccess);
        
        // Verify the first project no longer exists
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // First project should be gone
            var project1 = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == project1Id);
            Assert.Null(project1);
            
            // Second project should still exist
            var project2 = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == project2Id);
            Assert.NotNull(project2);
            Assert.Equal(project2Name, project2.Name);
        }
    }
    
    [Fact]
    public async Task RemoveProject_Handles_Complex_Hierarchy()
    {
        // Arrange - Create a project with complex hierarchy
        var projectName = "RemoveProject Test Project 7";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create multiple user stories with different numbers of tasks
        // UserStory 1: No tasks
        // UserStory 2: One task
        // UserStory 3: Multiple tasks
        var userStoryNames = new[] { 
            "RemoveProject Empty Story", 
            "RemoveProject Single Task Story", 
            "RemoveProject Multiple Tasks Story" 
        };
        
        var userStoryIds = new List<Guid>();
        
        foreach (var name in userStoryNames)
        {
            var createUserStoryRequest = new CreateUserStoryRequest(name, projectId);
            var userStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
            Assert.True(userStoryResult.IsSuccess);
            userStoryIds.Add(userStoryResult.Value);
        }
        
        // Add one task to the second user story
        var createTask1Request = new CreateTaskRequest(
            "RemoveProject Single Task", 
            "Single task prompt", 
            userStoryIds[1]);
        await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        
        // Add multiple tasks to the third user story
        for (int i = 1; i <= 3; i++)
        {
            var createTaskRequest = new CreateTaskRequest(
                $"RemoveProject Multiple Task {i}", 
                $"Multiple task prompt {i}", 
                userStoryIds[2]);
            await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        }
        
        // Act - Remove the project with cascadeDelete=true
        var removeProjectRequest = new RemoveProjectRequest(projectId, cascadeDelete: true);
        var removeProjectResult = await _removeProjectHandler.Handle(removeProjectRequest, CancellationToken.None);
        
        // Print any error messages
        if (!removeProjectResult.IsSuccess && removeProjectResult.Errors.Any())
        {
            Console.WriteLine($"Error: {removeProjectResult.Errors.First().Message}");
        }
        
        // Assert - Verify removal success
        Assert.True(removeProjectResult.IsSuccess);
        
        // Verify the entire hierarchy is removed
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            // Project should be gone
            var project = await context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.Null(project);
            
            // All user stories should be gone
            foreach (var userStoryId in userStoryIds)
            {
                var userStory = await context.UserStories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(us => us.Id == userStoryId);
                Assert.Null(userStory);
            }
            
            // All tasks should be gone
            var tasksCount = await context.Tasks
                .AsNoTracking()
                .Where(t => userStoryIds.Contains(t.UserStoryId))
                .CountAsync();
            Assert.Equal(0, tasksCount);
        }
    }
}