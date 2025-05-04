using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.UpdateProject;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task Update_Project_Name_Succeeds()
    {
        // Arrange - First create a project
        var projectName = "OriginalName";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createResult.IsSuccess);
        var projectId = createResult.Value;
        
        // Arrange - Then prepare the update
        var newName = "UpdatedName";
        var updateProjectRequest = new UpdateProjectRequest(projectId, newName);
        
        // Act - Update the project
        var updateResult = await _updateProjectHandler.Handle(updateProjectRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made

        // Create a fresh database context to verify the changes        
        using (var verificationContext = new DataContext(
            new DbContextOptionsBuilder<DataContext>()
                .UseNpgsql(TransactionalTestDatabaseFixture.ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                .Options))
        {
            // Get the updated project from a fresh context
            var updatedProject = await verificationContext.Projects.FindAsync(projectId);
            
            // Verify name was updated
            Assert.NotNull(updatedProject);
            Assert.Equal(newName, updatedProject.Name);
        }
    }
    
    [Fact]
    public async Task Update_Project_With_Same_Name_Returns_NoChange()
    {
        // Arrange - First create a project
        var projectName = "TestProject";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createResult.IsSuccess);
        var projectId = createResult.Value;
        
        // Arrange - Then prepare update with same name
        var updateProjectRequest = new UpdateProjectRequest(projectId, projectName);
        
        // Act - Update the project with the same name
        var updateResult = await _updateProjectHandler.Handle(updateProjectRequest, CancellationToken.None);
        
        // Assert - Verify success but no changes made
        Assert.True(updateResult.IsSuccess);
        Assert.False(updateResult.Value); // Should return false indicating no change was made
    }
    
    [Fact]
    public async Task Update_NonExistent_Project_Fails()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateProjectRequest = new UpdateProjectRequest(nonExistentId, "NewName");
        
        // Act
        var result = await _updateProjectHandler.Handle(updateProjectRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Project with ID '{nonExistentId}'", result.Errors.First().Message);
    }
}