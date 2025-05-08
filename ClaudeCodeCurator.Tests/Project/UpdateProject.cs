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
        
        // Fetch and verify initial value
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var project = await context.Projects.FindAsync(projectId);
            Assert.NotNull(project);
            Assert.Equal(projectName, project.Name);
        }
        
        // Arrange - Then prepare the update
        var newName = "UpdatedName";
        var updateProjectRequest = new UpdateProjectRequest(projectId, newName);
        
        // Act - Update the project
        var updateResult = await _updateProjectHandler.Handle(updateProjectRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the project was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            // Need to explicitly clear the EF Core cache to see the latest changes
            context.ChangeTracker.Clear();
            
            var updatedProject = await context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
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
    
    [Fact]
    public async Task Update_Project_VerifyPrompt_Succeeds()
    {
        // Arrange - First create a project
        var projectName = "ProjectWithPrompt";
        var primePrompt = "Initial prime prompt";
        var verifyPrompt = "Initial verify prompt";
        var createProjectRequest = new CreateProjectRequest(projectName, primePrompt, verifyPrompt);
        var createResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createResult.IsSuccess);
        var projectId = createResult.Value;
        
        // Fetch and verify initial values
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var project = await context.Projects.FindAsync(projectId);
            Assert.NotNull(project);
            Assert.Equal(projectName, project.Name);
            Assert.Equal(primePrompt, project.PrimePrompt);
            Assert.Equal(verifyPrompt, project.VerifyPrompt);
        }
        
        // Arrange - Then prepare the update
        var newVerifyPrompt = "Updated verify prompt";
        var updateProjectRequest = new UpdateProjectRequest(projectId, projectName, primePrompt, newVerifyPrompt);
        
        // Act - Update the project
        var updateResult = await _updateProjectHandler.Handle(updateProjectRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the project was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedProject = await context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.NotNull(updatedProject);
            Assert.Equal(projectName, updatedProject.Name);
            Assert.Equal(primePrompt, updatedProject.PrimePrompt);
            Assert.Equal(newVerifyPrompt, updatedProject.VerifyPrompt);
        }
    }
    
    [Fact]
    public async Task Update_Project_PrimePrompt_And_VerifyPrompt_Succeeds()
    {
        // Arrange - First create a project with no prompts
        var projectName = "ProjectWithoutPrompts";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createResult.IsSuccess);
        var projectId = createResult.Value;
        
        // Fetch and verify initial values
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var project = await context.Projects.FindAsync(projectId);
            Assert.NotNull(project);
            Assert.Equal(projectName, project.Name);
            Assert.Null(project.PrimePrompt);
            Assert.Null(project.VerifyPrompt);
        }
        
        // Arrange - Then prepare the update with both prompts
        var newPrimePrompt = "New prime prompt";
        var newVerifyPrompt = "New verify prompt";
        var updateProjectRequest = new UpdateProjectRequest(projectId, projectName, newPrimePrompt, newVerifyPrompt);
        
        // Act - Update the project
        var updateResult = await _updateProjectHandler.Handle(updateProjectRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the project was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedProject = await context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            Assert.NotNull(updatedProject);
            Assert.Equal(projectName, updatedProject.Name);
            Assert.Equal(newPrimePrompt, updatedProject.PrimePrompt);
            Assert.Equal(newVerifyPrompt, updatedProject.VerifyPrompt);
        }
    }
}