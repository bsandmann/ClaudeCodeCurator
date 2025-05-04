using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.UpdateTask;
using ClaudeCodeCurator.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaudeCodeCurator.Tests;

public partial class IntegrationTests
{
    [Fact]
    public async Task Update_Task_Name_Succeeds()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Task Update";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Task Update";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var originalTaskName = "Original Task Name";
        var originalPromptBody = "Original prompt body";
        var originalType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(
            originalTaskName, 
            originalPromptBody, 
            userStoryId, 
            originalType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Arrange - Prepare the update
        var newTaskName = "Updated Task Name";
        var updateTaskRequest = new UpdateTaskRequest(
            taskId, 
            newTaskName, 
            originalPromptBody, // Keep the same prompt
            originalType);      // Keep the same type
        
        // Act - Update the task
        var updateResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the task was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedTask = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.NotNull(updatedTask);
            Assert.Equal(newTaskName, updatedTask.Name);
            Assert.Equal(originalPromptBody, updatedTask.PromptBody); // PromptBody should be unchanged
            Assert.Equal(originalType, updatedTask.Type);            // Type should be unchanged
        }
    }
    
    [Fact]
    public async Task Update_Task_PromptBody_Succeeds()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Prompt Update";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Prompt Update";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Prompt Update";
        var originalPrompt = "Original prompt text";
        var createTaskRequest = new CreateTaskRequest(taskName, originalPrompt, userStoryId);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Arrange - Prepare the update (only changing prompt)
        var newPrompt = "This is an updated prompt text with more details";
        var updateTaskRequest = new UpdateTaskRequest(
            taskId, 
            taskName,       // Keep the same name
            newPrompt,      // Change prompt body
            TaskType.Task); // Keep the same type
        
        // Act - Update the task
        var updateResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the task was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedTask = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.NotNull(updatedTask);
            Assert.Equal(taskName, updatedTask.Name); // Name should be unchanged
            Assert.Equal(newPrompt, updatedTask.PromptBody);
        }
    }
    
    [Fact]
    public async Task Update_Task_Type_Succeeds()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Type Update";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Type Update";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Type Update";
        var promptBody = "Test prompt";
        var originalType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            originalType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Arrange - Prepare the update (only changing type)
        var newType = TaskType.Bug;
        var updateTaskRequest = new UpdateTaskRequest(
            taskId, 
            taskName,    // Keep the same name
            promptBody,  // Keep the same prompt
            newType);    // Change type to Bug
        
        // Act - Update the task
        var updateResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify the task was actually updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedTask = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.NotNull(updatedTask);
            Assert.Equal(taskName, updatedTask.Name);       // Name should be unchanged
            Assert.Equal(promptBody, updatedTask.PromptBody); // Prompt should be unchanged
            Assert.Equal(newType, updatedTask.Type);        // Type should be changed to Bug
        }
    }
    
    [Fact]
    public async Task Update_Task_With_Same_Values_Returns_NoChange()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for No Change";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for No Change";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for No Change";
        var promptBody = "Test prompt that won't change";
        var taskType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            taskType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Arrange - Prepare the update with the same values
        var updateTaskRequest = new UpdateTaskRequest(
            taskId, 
            taskName,   // Same name
            promptBody, // Same prompt
            taskType);  // Same type
        
        // Act - Update with the same values
        var updateResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        
        // Assert - Verify success but no changes
        Assert.True(updateResult.IsSuccess);
        Assert.False(updateResult.Value); // Should return false indicating no change was made
    }
    
    [Fact]
    public async Task Update_NonExistent_Task_Fails()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateTaskRequest = new UpdateTaskRequest(
            nonExistentId, 
            "New Name", 
            "New Prompt", 
            TaskType.Task);
        
        // Act
        var result = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task Update_Task_With_Duplicate_Name_In_Same_UserStory_Fails()
    {
        // Arrange - Create project and user story
        var projectName = "Project for Duplicate Task Name Test";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Duplicate Task Name";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create two tasks in the same user story
        var task1Name = "First Task";
        var task2Name = "Second Task";
        
        var createTask1Request = new CreateTaskRequest(task1Name, "Prompt 1", userStoryId);
        var createTask2Request = new CreateTaskRequest(task2Name, "Prompt 2", userStoryId);
        
        var task1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        var task2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        
        Assert.True(task1Result.IsSuccess);
        Assert.True(task2Result.IsSuccess);
        
        var task1Id = task1Result.Value;
        var task2Id = task2Result.Value;
        
        // Arrange - Try to update the second task to have the same name as the first
        var updateTaskRequest = new UpdateTaskRequest(
            task2Id, 
            task1Name, // Duplicate name
            "Updated prompt", 
            TaskType.Task);
        
        // Act
        var updateResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.False(updateResult.IsSuccess);
        Assert.Contains($"A different task with name '{task1Name}' already exists", 
            updateResult.Errors.First().Message);
    }
    
    [Fact]
    public async Task Update_All_Task_Properties_At_Once_Succeeds()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Full Update";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Full Update";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var originalName = "Original Task";
        var originalPrompt = "Original prompt";
        var originalType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(
            originalName, 
            originalPrompt, 
            userStoryId, 
            originalType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Arrange - Prepare the update with all values changed
        var newName = "Completely Updated Task";
        var newPrompt = "Completely new prompt text";
        var newType = TaskType.Verify;
        var updateTaskRequest = new UpdateTaskRequest(
            taskId, 
            newName, 
            newPrompt, 
            newType);
        
        // Act - Update all properties at once
        var updateResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        
        // Assert - Verify update success
        Assert.True(updateResult.IsSuccess);
        Assert.True(updateResult.Value); // Should return true indicating a change was made
        
        // Verify all properties were updated in the database
        using (var context = Fixture.CreateContext())
        {
            context.ChangeTracker.Clear();
            
            var updatedTask = await context.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId);
                
            Assert.NotNull(updatedTask);
            Assert.Equal(newName, updatedTask.Name);
            Assert.Equal(newPrompt, updatedTask.PromptBody);
            Assert.Equal(newType, updatedTask.Type);
        }
    }
}