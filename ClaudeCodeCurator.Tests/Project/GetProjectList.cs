using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.GetProjectList;
using ClaudeCodeCurator.Models;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task GetProjectList_Returns_Empty_List_When_No_Projects_Exist()
    {
        // First clean up any projects that might exist from previous tests
        await CleanupProjects();
        
        // Act
        var getProjectListRequest = new GetProjectListRequest();
        var result = await _getProjectListHandler.Handle(getProjectListRequest, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetProjectList_Returns_All_Projects()
    {
        // First clean up any projects that might exist from previous tests
        await CleanupProjects();
        
        // Arrange - Create multiple projects
        var project1Name = "GPL Test Project 1";
        var project2Name = "GPL Test Project 2";
        var project3Name = "GPL Test Project 3";

        var createProject1Request = new CreateProjectRequest(project1Name);
        var createProject2Request = new CreateProjectRequest(project2Name);
        var createProject3Request = new CreateProjectRequest(project3Name);
        
        var createProject1Result = await _createProjectHandler.Handle(createProject1Request, CancellationToken.None);
        var createProject2Result = await _createProjectHandler.Handle(createProject2Request, CancellationToken.None);
        var createProject3Result = await _createProjectHandler.Handle(createProject3Request, CancellationToken.None);

        Assert.True(createProject1Result.IsSuccess);
        Assert.True(createProject2Result.IsSuccess);
        Assert.True(createProject3Result.IsSuccess);

        var project1Id = createProject1Result.Value;
        var project2Id = createProject2Result.Value;
        var project3Id = createProject3Result.Value;

        // Act
        var getProjectListRequest = new GetProjectListRequest();
        var result = await _getProjectListHandler.Handle(getProjectListRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        // Should have exactly 3 projects
        Assert.Equal(3, result.Value.Count);
        
        // Verify the 3 projects we created are in the list
        var projectNames = result.Value.Select(p => p.Name).ToList();
        Assert.Contains(project1Name, projectNames);
        Assert.Contains(project2Name, projectNames);
        Assert.Contains(project3Name, projectNames);
        
        // Verify projects have the correct IDs
        var project1 = result.Value.FirstOrDefault(p => p.Name == project1Name);
        var project2 = result.Value.FirstOrDefault(p => p.Name == project2Name);
        var project3 = result.Value.FirstOrDefault(p => p.Name == project3Name);
        
        Assert.NotNull(project1);
        Assert.NotNull(project2);
        Assert.NotNull(project3);
        
        Assert.Equal(project1Id, project1.Id);
        Assert.Equal(project2Id, project2.Id);
        Assert.Equal(project3Id, project3.Id);
    }

    [Fact]
    public async Task GetProjectList_Returns_Projects_With_Empty_UserStories_Collections()
    {
        // First clean up any projects that might exist from previous tests
        await CleanupProjects();
        
        // Arrange - Create a project
        var projectName = "GPL Test Project 4";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        Assert.True(createProjectResult.IsSuccess);

        // Act
        var getProjectListRequest = new GetProjectListRequest();
        var result = await _getProjectListHandler.Handle(getProjectListRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Should have exactly 1 project
        Assert.Single(result.Value);
        
        // Get the project
        var project = result.Value.First();
        Assert.Equal(projectName, project.Name);
        
        // Verify it has an empty UserStories collection (not null)
        Assert.NotNull(project.UserStories);
        Assert.Empty(project.UserStories);
    }

    [Fact]
    public async Task GetProjectList_Returns_Projects_With_Correct_Structure()
    {
        // First clean up any projects that might exist from previous tests
        await CleanupProjects();
        
        // Arrange - Create a project
        var projectName = "GPL Test Project 5";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;

        // Act
        var getProjectListRequest = new GetProjectListRequest();
        var result = await _getProjectListHandler.Handle(getProjectListRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Should have exactly 1 project
        Assert.Single(result.Value);
        
        // Get the project
        var project = result.Value.First();
        
        // Verify it has the correct structure
        Assert.IsType<ProjectModel>(project);
        Assert.Equal(projectName, project.Name);
        Assert.Equal(projectId, project.Id);
        Assert.NotEqual(default, project.CreatedOrUpdatedUtc);
        
        // Ensure UserStories collection is always initialized (not null)
        Assert.NotNull(project.UserStories);
    }
    
    private async Task CleanupProjects()
    {
        // Use the TransactionalTestDatabaseFixture's Cleanup method
        Fixture.Cleanup();
        
        // Wait a bit to ensure cleanup completes
        await Task.Delay(100);
    }
}