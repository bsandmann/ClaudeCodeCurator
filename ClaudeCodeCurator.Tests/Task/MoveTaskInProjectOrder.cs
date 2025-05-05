using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.MoveTaskInProjectOrder;
using ClaudeCodeCurator.Commands.SetUserTaskApproval;
using ClaudeCodeCurator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task MoveTaskInProjectOrder_Should_Move_Task_To_Top()
    {
        // Arrange - Create project, user stories, and tasks
        var projectName = "Test Project for Order";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Order";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create and approve three tasks
        var task1Name = "Task 1 for Order";
        var task1Request = new CreateTaskRequest(task1Name, "Prompt 1", userStoryId);
        var task1Result = await _createTaskHandler.Handle(task1Request, CancellationToken.None);
        Assert.True(task1Result.IsSuccess);
        var task1Id = task1Result.Value;
        
        var task2Name = "Task 2 for Order";
        var task2Request = new CreateTaskRequest(task2Name, "Prompt 2", userStoryId);
        var task2Result = await _createTaskHandler.Handle(task2Request, CancellationToken.None);
        Assert.True(task2Result.IsSuccess);
        var task2Id = task2Result.Value;
        
        var task3Name = "Task 3 for Order";
        var task3Request = new CreateTaskRequest(task3Name, "Prompt 3", userStoryId);
        var task3Result = await _createTaskHandler.Handle(task3Request, CancellationToken.None);
        Assert.True(task3Result.IsSuccess);
        var task3Id = task3Result.Value;
        
        // Approve all tasks to add them to the ordered list
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task1Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task2Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task3Id, true, projectId), CancellationToken.None);
        
        // Get initial positions
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var initialPositions = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .ToListAsync();
                
            Assert.Equal(3, initialPositions.Count);
            Assert.Equal(task1Id, initialPositions[0].TaskId);
            Assert.Equal(task2Id, initialPositions[1].TaskId);
            Assert.Equal(task3Id, initialPositions[2].TaskId);
        }
        
        // Act - Move the last task to the top
        var moveRequest = new MoveTaskInProjectOrderRequest(projectId, task3Id, PositionType.ToTop);
        var moveResult = await _moveTaskInProjectOrderHandler.Handle(moveRequest, CancellationToken.None);
        
        // Assert
        Assert.True(moveResult.IsSuccess);
        
        // Verify the new positions
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var afterPositions = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .ToListAsync();
                
            Assert.Equal(3, afterPositions.Count);
            Assert.Equal(task3Id, afterPositions[0].TaskId);
            Assert.Equal(task1Id, afterPositions[1].TaskId);
            Assert.Equal(task2Id, afterPositions[2].TaskId);
        }
    }
    
    [Fact]
    public async Task MoveTaskInProjectOrder_Should_Move_Task_To_Bottom()
    {
        // Arrange - Create project, user stories, and tasks
        var projectName = "Test Project for Bottom";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Bottom";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create and approve three tasks
        var task1Name = "Task 1 for Bottom";
        var task1Request = new CreateTaskRequest(task1Name, "Prompt 1", userStoryId);
        var task1Result = await _createTaskHandler.Handle(task1Request, CancellationToken.None);
        Assert.True(task1Result.IsSuccess);
        var task1Id = task1Result.Value;
        
        var task2Name = "Task 2 for Bottom";
        var task2Request = new CreateTaskRequest(task2Name, "Prompt 2", userStoryId);
        var task2Result = await _createTaskHandler.Handle(task2Request, CancellationToken.None);
        Assert.True(task2Result.IsSuccess);
        var task2Id = task2Result.Value;
        
        var task3Name = "Task 3 for Bottom";
        var task3Request = new CreateTaskRequest(task3Name, "Prompt 3", userStoryId);
        var task3Result = await _createTaskHandler.Handle(task3Request, CancellationToken.None);
        Assert.True(task3Result.IsSuccess);
        var task3Id = task3Result.Value;
        
        // Approve all tasks to add them to the ordered list
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task1Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task2Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task3Id, true, projectId), CancellationToken.None);
        
        // Get initial positions
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var initialPositions = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .ToListAsync();
                
            Assert.Equal(3, initialPositions.Count);
        }
        
        // Act - Move the first task to the bottom
        var moveRequest = new MoveTaskInProjectOrderRequest(projectId, task1Id, PositionType.ToBottom);
        var moveResult = await _moveTaskInProjectOrderHandler.Handle(moveRequest, CancellationToken.None);
        
        // Assert
        Assert.True(moveResult.IsSuccess);
        
        // Verify the new positions
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var afterPositions = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .ToListAsync();
                
            Assert.Equal(3, afterPositions.Count);
            Assert.Equal(task2Id, afterPositions[0].TaskId);
            Assert.Equal(task3Id, afterPositions[1].TaskId);
            Assert.Equal(task1Id, afterPositions[2].TaskId);
        }
    }
    
    [Fact]
    public async Task MoveTaskInProjectOrder_Should_Move_Task_Before_Reference()
    {
        // Arrange - Create project, user stories, and tasks
        var projectName = "Test Project for Before";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Before";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create and approve three tasks
        var task1Name = "Task 1 for Before";
        var task1Request = new CreateTaskRequest(task1Name, "Prompt 1", userStoryId);
        var task1Result = await _createTaskHandler.Handle(task1Request, CancellationToken.None);
        Assert.True(task1Result.IsSuccess);
        var task1Id = task1Result.Value;
        
        var task2Name = "Task 2 for Before";
        var task2Request = new CreateTaskRequest(task2Name, "Prompt 2", userStoryId);
        var task2Result = await _createTaskHandler.Handle(task2Request, CancellationToken.None);
        Assert.True(task2Result.IsSuccess);
        var task2Id = task2Result.Value;
        
        var task3Name = "Task 3 for Before";
        var task3Request = new CreateTaskRequest(task3Name, "Prompt 3", userStoryId);
        var task3Result = await _createTaskHandler.Handle(task3Request, CancellationToken.None);
        Assert.True(task3Result.IsSuccess);
        var task3Id = task3Result.Value;
        
        // Approve all tasks to add them to the ordered list
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task1Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task2Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task3Id, true, projectId), CancellationToken.None);
        
        // Act - Move the third task before the second task
        var moveRequest = new MoveTaskInProjectOrderRequest(projectId, task3Id, PositionType.BeforeTask, task2Id);
        var moveResult = await _moveTaskInProjectOrderHandler.Handle(moveRequest, CancellationToken.None);
        
        // Assert
        Assert.True(moveResult.IsSuccess);
        
        // Verify the new positions
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var afterPositions = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .ToListAsync();
                
            Assert.Equal(3, afterPositions.Count);
            Assert.Equal(task1Id, afterPositions[0].TaskId);
            Assert.Equal(task3Id, afterPositions[1].TaskId);
            Assert.Equal(task2Id, afterPositions[2].TaskId);
        }
    }
    
    [Fact]
    public async Task MoveTaskInProjectOrder_Should_Move_Task_After_Reference()
    {
        // Arrange - Create project, user stories, and tasks
        var projectName = "Test Project for After";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for After";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create and approve three tasks
        var task1Name = "Task 1 for After";
        var task1Request = new CreateTaskRequest(task1Name, "Prompt 1", userStoryId);
        var task1Result = await _createTaskHandler.Handle(task1Request, CancellationToken.None);
        Assert.True(task1Result.IsSuccess);
        var task1Id = task1Result.Value;
        
        var task2Name = "Task 2 for After";
        var task2Request = new CreateTaskRequest(task2Name, "Prompt 2", userStoryId);
        var task2Result = await _createTaskHandler.Handle(task2Request, CancellationToken.None);
        Assert.True(task2Result.IsSuccess);
        var task2Id = task2Result.Value;
        
        var task3Name = "Task 3 for After";
        var task3Request = new CreateTaskRequest(task3Name, "Prompt 3", userStoryId);
        var task3Result = await _createTaskHandler.Handle(task3Request, CancellationToken.None);
        Assert.True(task3Result.IsSuccess);
        var task3Id = task3Result.Value;
        
        // Approve all tasks to add them to the ordered list
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task1Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task2Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task3Id, true, projectId), CancellationToken.None);
        
        // Act - Move the first task after the second task
        var moveRequest = new MoveTaskInProjectOrderRequest(projectId, task1Id, PositionType.AfterTask, task2Id);
        var moveResult = await _moveTaskInProjectOrderHandler.Handle(moveRequest, CancellationToken.None);
        
        // Assert
        Assert.True(moveResult.IsSuccess);
        
        // Verify the new positions
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var afterPositions = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .ToListAsync();
                
            Assert.Equal(3, afterPositions.Count);
            Assert.Equal(task2Id, afterPositions[0].TaskId);
            Assert.Equal(task1Id, afterPositions[1].TaskId);
            Assert.Equal(task3Id, afterPositions[2].TaskId);
        }
    }
    
    [Fact]
    public async Task MoveTaskInProjectOrder_Should_Move_Task_To_Specific_Position()
    {
        // Arrange - Create project, user stories, and tasks
        var projectName = "Test Project for Position";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Position";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create and approve three tasks
        var task1Name = "Task 1 for Position";
        var task1Request = new CreateTaskRequest(task1Name, "Prompt 1", userStoryId);
        var task1Result = await _createTaskHandler.Handle(task1Request, CancellationToken.None);
        Assert.True(task1Result.IsSuccess);
        var task1Id = task1Result.Value;
        
        var task2Name = "Task 2 for Position";
        var task2Request = new CreateTaskRequest(task2Name, "Prompt 2", userStoryId);
        var task2Result = await _createTaskHandler.Handle(task2Request, CancellationToken.None);
        Assert.True(task2Result.IsSuccess);
        var task2Id = task2Result.Value;
        
        var task3Name = "Task 3 for Position";
        var task3Request = new CreateTaskRequest(task3Name, "Prompt 3", userStoryId);
        var task3Result = await _createTaskHandler.Handle(task3Request, CancellationToken.None);
        Assert.True(task3Result.IsSuccess);
        var task3Id = task3Result.Value;
        
        // Approve all tasks to add them to the ordered list
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task1Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task2Id, true, projectId), CancellationToken.None);
        await _setUserTaskApprovalHandler.Handle(new SetUserTaskApprovalRequest(task3Id, true, projectId), CancellationToken.None);
        
        // Get the initial position of the first task
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var initialPositions = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId)
                .OrderBy(o => o.Position)
                .ToListAsync();
                
            Assert.Equal(3, initialPositions.Count);
        }
        
        // Act - Move the first task to a specific position (999)
        var moveRequest = new MoveTaskInProjectOrderRequest(projectId, task1Id, 999);
        var moveResult = await _moveTaskInProjectOrderHandler.Handle(moveRequest, CancellationToken.None);
        
        // Assert
        Assert.True(moveResult.IsSuccess);
        
        // Verify the new positions
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            var task1Position = await context.ProjectTaskOrders
                .Where(o => o.ProjectId == projectId && o.TaskId == task1Id)
                .Select(o => o.Position)
                .FirstOrDefaultAsync();
                
            Assert.Equal(999, task1Position);
        }
    }
    
    [Fact]
    public async Task MoveTaskInProjectOrder_Should_Return_Failure_When_Task_Not_In_OrderedList()
    {
        // Arrange - Create project, user story, and task, but don't approve it (so it's not in ordered list)
        var projectName = "Test Project for Failure";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Failure";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Failure";
        var taskRequest = new CreateTaskRequest(taskName, "Prompt for failure", userStoryId);
        var taskResult = await _createTaskHandler.Handle(taskRequest, CancellationToken.None);
        Assert.True(taskResult.IsSuccess);
        var taskId = taskResult.Value;
        
        // Don't approve the task, so it's not in the ordered list
        
        // Act - Try to move the task to the top
        var moveRequest = new MoveTaskInProjectOrderRequest(projectId, taskId, PositionType.ToTop);
        var moveResult = await _moveTaskInProjectOrderHandler.Handle(moveRequest, CancellationToken.None);
        
        // Assert
        Assert.False(moveResult.IsSuccess);
        Assert.Contains($"Task with ID '{taskId}' is not in the ordered list", moveResult.Errors.First().Message);
    }
}