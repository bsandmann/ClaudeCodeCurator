using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.UpdateUserStory;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task Update_UserStory_Name_Succeeds()
    {
        // Arrange - First create a project and user story
        var projectName = "Test Project for Update";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var originalUserStoryName = "Original User Story";
        var originalDescription = "Original Description";
        var createUserStoryRequest = new CreateUserStoryRequest(
            originalUserStoryName, 
            projectId, 
            originalDescription);
        
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Arrange - Prepare the update
        var newUserStoryName = "Updated User Story";
        var updateUserStoryRequest = new UpdateUserStoryRequest(
            userStoryId, 
            newUserStoryName, 
            originalDescription); // Keep the same description
        
        // Act - Update the user story
        var updateResult = await _updateUserStoryHandler.Handle(updateUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the user story was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedUserStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.NotNull(updatedUserStory);
            Assert.Equal(newUserStoryName, updatedUserStory.Name);
            Assert.Equal(originalDescription, updatedUserStory.Description); // Description should be unchanged
        }
    }
    
    [Fact]
    public async Task Update_UserStory_Description_Succeeds()
    {
        // Arrange - First create a project and user story
        var projectName = "Test Project for Description Update";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Description Test";
        var originalDescription = "Original Description";
        var createUserStoryRequest = new CreateUserStoryRequest(
            userStoryName, 
            projectId, 
            originalDescription);
        
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Arrange - Prepare the update (only changing description)
        var newDescription = "This is an updated description with more details";
        var updateUserStoryRequest = new UpdateUserStoryRequest(
            userStoryId, 
            userStoryName, // Keep the same name
            newDescription);
        
        // Act - Update the user story
        var updateResult = await _updateUserStoryHandler.Handle(updateUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the user story was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedUserStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.NotNull(updatedUserStory);
            Assert.Equal(userStoryName, updatedUserStory.Name); // Name should be unchanged
            Assert.Equal(newDescription, updatedUserStory.Description);
        }
    }
    
    [Fact]
    public async Task Update_UserStory_With_Same_Values_Returns_NoChange()
    {
        // Arrange - First create a project and user story
        var projectName = "Test Project for No Change Test";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for No Change Test";
        var description = "Description that won't change";
        var createUserStoryRequest = new CreateUserStoryRequest(
            userStoryName, 
            projectId, 
            description);
        
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Arrange - Prepare the update with the same values
        var updateUserStoryRequest = new UpdateUserStoryRequest(
            userStoryId, 
            userStoryName, // Same name
            description);  // Same description
        
        // Act - Update with the same values
        var updateResult = await _updateUserStoryHandler.Handle(updateUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify success but no changes
        Assert.True(updateResult.IsSuccess);
        Assert.False(updateResult.Value); // Should return false indicating no change was made
    }
    
    [Fact]
    public async Task Update_UserStory_With_Null_Description_Succeeds()
    {
        // Arrange - First create a project and user story with a description
        var projectName = "Test Project for Null Description Test";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story with Description";
        var originalDescription = "This will be set to null";
        var createUserStoryRequest = new CreateUserStoryRequest(
            userStoryName, 
            projectId, 
            originalDescription);
        
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Arrange - Prepare the update with null description
        var updateUserStoryRequest = new UpdateUserStoryRequest(
            userStoryId, 
            userStoryName, // Same name
            null);         // Null description
        
        // Act - Update with null description
        var updateResult = await _updateUserStoryHandler.Handle(updateUserStoryRequest, CancellationToken.None);
        
        // Assert - Verify success with change
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the description was set to null
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedUserStory = await context.UserStories
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.Id == userStoryId);
                
            Assert.NotNull(updatedUserStory);
            Assert.Null(updatedUserStory.Description);
        }
    }
    
    [Fact]
    public async Task Update_NonExistent_UserStory_Fails()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateUserStoryRequest = new UpdateUserStoryRequest(
            nonExistentId, 
            "New Name", 
            "New Description");
        
        // Act
        var result = await _updateUserStoryHandler.Handle(updateUserStoryRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"User story with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task Update_UserStory_With_Duplicate_Name_In_Same_Project_Fails()
    {
        // Arrange - First create a project
        var projectName = "Project for Duplicate Name Test";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create two user stories in the same project
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
        
        // Arrange - Try to update the second user story to have the same name as the first
        var updateUserStoryRequest = new UpdateUserStoryRequest(
            userStory2Id, 
            userStory1Name, // Duplicate name
            "Some description");
        
        // Act
        var updateResult = await _updateUserStoryHandler.Handle(updateUserStoryRequest, CancellationToken.None);
        
        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Contains($"A different user story with name '{userStory1Name}' already exists", 
            updateResult.Errors.First().Message);
    }
}