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
}