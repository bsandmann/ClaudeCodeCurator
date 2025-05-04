using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateUserStory;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task CreateUserStory_Succeeds_For_Default_Case()
    {
        // Arrange - First create a project to attach the user story to
        var projectName = "Test Project";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create a user story
        var userStoryName = "Test User Story";
        var userStoryDescription = "This is a test description";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId, userStoryDescription);
        
        // Act
        var result = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        var userStoryId = result.Value;
        Assert.NotEqual(Guid.Empty, userStoryId);
        
        // Verify the user story was created in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var userStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.NotNull(userStory);
            Assert.Equal(userStoryName, userStory.Name);
            Assert.Equal(userStoryDescription, userStory.Description);
            Assert.Equal(projectId, userStory.ProjectId);
        }
    }
    
    [Fact]
    public async Task CreateUserStory_Fails_For_NonExistent_Project()
    {
        // Arrange
        var nonExistentProjectId = Guid.NewGuid();
        var createUserStoryRequest = new CreateUserStoryRequest(
            "User Story with Invalid Project", 
            nonExistentProjectId);
        
        // Act
        var result = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Project with ID '{nonExistentProjectId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task CreateUserStory_Fails_For_Duplicate_Name_In_Same_Project()
    {
        // Arrange - First create a project
        var projectName = "Project for Duplicate Test";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create a user story
        var userStoryName = "Duplicate User Story";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        
        // First creation should succeed
        var firstResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(firstResult.IsSuccess);
        
        // Act - Try to create a second user story with the same name in the same project
        var secondResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        // Assert
        Assert.False(secondResult.IsSuccess);
        Assert.Contains($"User story with name '{userStoryName}' already exists", 
            secondResult.Errors.First().Message);
    }
    
    [Fact]
    public async Task CreateUserStory_Succeeds_With_Same_Name_In_Different_Projects()
    {
        // Arrange - Create two different projects
        var projectName1 = "Project One";
        var projectName2 = "Project Two";
        
        var createProject1Request = new CreateProjectRequest(projectName1);
        var createProject2Request = new CreateProjectRequest(projectName2);
        
        var project1Result = await _createProjectHandler.Handle(createProject1Request, CancellationToken.None);
        var project2Result = await _createProjectHandler.Handle(createProject2Request, CancellationToken.None);
        
        Assert.True(project1Result.IsSuccess);
        Assert.True(project2Result.IsSuccess);
        
        var project1Id = project1Result.Value;
        var project2Id = project2Result.Value;
        
        // Create a user story with the same name in both projects
        var userStoryName = "Common User Story Name";
        var createUserStory1 = new CreateUserStoryRequest(userStoryName, project1Id);
        var createUserStory2 = new CreateUserStoryRequest(userStoryName, project2Id);
        
        // Act
        var result1 = await _createUserStoryHandler.Handle(createUserStory1, CancellationToken.None);
        var result2 = await _createUserStoryHandler.Handle(createUserStory2, CancellationToken.None);
        
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        // Verify both user stories were created
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var userStory1 = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == result1.Value);
                
            var userStory2 = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == result2.Value);
            
            Assert.NotNull(userStory1);
            Assert.NotNull(userStory2);
            Assert.Equal(userStoryName, userStory1.Name);
            Assert.Equal(userStoryName, userStory2.Name);
            Assert.Equal(project1Id, userStory1.ProjectId);
            Assert.Equal(project2Id, userStory2.ProjectId);
        }
    }
}