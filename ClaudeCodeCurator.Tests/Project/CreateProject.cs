using ClaudeCodeCurator.Commands.CreateProject;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task CreateBlock_Succeeds_For_Default_Case()
    {
        // Arrange
        var createProjectRequest = new CreateProjectRequest("testname");

        // Act
        var result = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);

        // Assert
        Assert.True((bool)result.IsSuccess);
    }
    
    [Fact]
    public async Task Create_Project_With_VerifyPrompt_Succeeds()
    {
        // Arrange
        var projectName = "Project with VerifyPrompt";
        var verifyPrompt = "This is a verify prompt";
        var createProjectRequest = new CreateProjectRequest(projectName, verifyPrompt: verifyPrompt);

        // Act
        var result = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var projectId = result.Value;
        
        // Verify the project was actually created with verify prompt in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var project = await context.Projects.FindAsync(projectId);
            Assert.NotNull(project);
            Assert.Equal(projectName, project.Name);
            Assert.Equal(verifyPrompt, project.VerifyPrompt);
        }
    }
    
    [Fact]
    public async Task Create_Project_With_PrimePrompt_And_VerifyPrompt_Succeeds()
    {
        // Arrange
        var projectName = "Project with Both Prompts";
        var primePrompt = "This is a prime prompt";
        var verifyPrompt = "This is a verify prompt";
        var createProjectRequest = new CreateProjectRequest(projectName, primePrompt, verifyPrompt);

        // Act
        var result = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var projectId = result.Value;
        
        // Verify the project was created with both prompts in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var project = await context.Projects.FindAsync(projectId);
            Assert.NotNull(project);
            Assert.Equal(projectName, project.Name);
            Assert.Equal(primePrompt, project.PrimePrompt);
            Assert.Equal(verifyPrompt, project.VerifyPrompt);
        }
    }
}