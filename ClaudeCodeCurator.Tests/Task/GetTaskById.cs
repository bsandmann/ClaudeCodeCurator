using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Entities;
using ClaudeCodeCurator.Models;

namespace ClaudeCodeCurator.Tests;

using Commands.UpdateTask;

public partial class IntegrationTests
{
    [Fact]
    public async Task GetTaskById_Returns_Task_When_Exists()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for GetTaskById";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for GetTaskById";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for GetTaskById";
        var promptBody = "Test prompt for retrieval";
        var taskType = TaskType.Task;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            taskType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify retrieval success
        Assert.True(getTaskResult.IsSuccess);
        
        // Verify the task model has the correct properties
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(taskId, taskModel.Id);
        Assert.Equal(taskName, taskModel.Name);
        Assert.Equal(promptBody, taskModel.PromptBody);
        Assert.Equal(taskType, taskModel.Type);
        Assert.Equal(userStoryId, taskModel.UserStoryId);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Failure_When_Task_Not_Found()
    {
        // Arrange - Use a non-existent task ID
        var nonExistentId = Guid.NewGuid();
        var getTaskRequest = new GetTaskByIdRequest(nonExistentId);
        
        // Act
        var result = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Task with ID '{nonExistentId}'", result.Errors.First().Message);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Correct_TaskType_For_Bug()
    {
        // Arrange - Create project, user story, and bug task
        var projectName = "Test Project for Bug Task";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Bug Task";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task with Bug type
        var taskName = "Bug Task";
        var promptBody = "This is a bug report";
        var taskType = TaskType.Bug;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            taskType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the bug task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model has Bug type
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(TaskType.Bug, taskModel.Type);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Correct_TaskType_For_Verify()
    {
        // Arrange - Create project, user story, and verification task
        var projectName = "Test Project for Verify Task";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Verify Task";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task with Verify type
        var taskName = "Verification Task";
        var promptBody = "This is a verification task";
        var taskType = TaskType.Verify;
        var createTaskRequest = new CreateTaskRequest(
            taskName, 
            promptBody, 
            userStoryId, 
            taskType);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the verify task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model has Verify type
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(TaskType.Verify, taskModel.Type);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_Correct_Model_After_Task_Update()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Updated Task";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Updated Task";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create a task
        var originalName = "Original Task Name";
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
        
        // Update the task
        var newName = "Updated Task Name";
        var newPrompt = "Updated prompt text";
        var newType = TaskType.Bug;
        var updateTaskRequest = new UpdateTaskRequest(
            taskId, 
            newName, 
            newPrompt, 
            newType);
            
        var updateTaskResult = await _updateTaskHandler.Handle(updateTaskRequest, CancellationToken.None);
        Assert.True(updateTaskResult.IsSuccess);
        
        // Act - Get the updated task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model reflects the updates
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        Assert.NotNull(taskModel);
        Assert.Equal(newName, taskModel.Name);
        Assert.Equal(newPrompt, taskModel.PromptBody);
        Assert.Equal(newType, taskModel.Type);
    }
    
    [Fact]
    public async Task GetTaskById_Returns_TaskModel_With_Correct_Structure()
    {
        // Arrange - Create project, user story, and task
        var projectName = "Test Project for Model Structure";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "User Story for Model Structure";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        var taskName = "Task for Model Structure";
        var promptBody = "Test prompt for model structure";
        var createTaskRequest = new CreateTaskRequest(taskName, promptBody, userStoryId);
            
        var createTaskResult = await _createTaskHandler.Handle(createTaskRequest, CancellationToken.None);
        Assert.True(createTaskResult.IsSuccess);
        var taskId = createTaskResult.Value;
        
        // Act - Get the task by ID
        var getTaskRequest = new GetTaskByIdRequest(taskId);
        var getTaskResult = await _getTaskByIdHandler.Handle(getTaskRequest, CancellationToken.None);
        
        // Assert - Verify the task model has the correct structure
        Assert.True(getTaskResult.IsSuccess);
        var taskModel = getTaskResult.Value;
        
        // Check that the model is of type TaskModel
        Assert.IsType<TaskModel>(taskModel);
        
        // Verify the model has all required properties
        Assert.NotEqual(Guid.Empty, taskModel.Id);
        Assert.NotEmpty(taskModel.Name);
        Assert.NotEmpty(taskModel.PromptBody);
        Assert.NotEqual(Guid.Empty, taskModel.UserStoryId);
        
        // Verify the model doesn't include any entity references
        var taskModelType = typeof(TaskModel);
        var properties = taskModelType.GetProperties();
        
        // The TaskModel has 16 properties
        Assert.Equal(16, properties.Length);
        
        // Check that there are no navigation properties to other entities
        var propertyNames = properties.Select(p => p.Name).ToList();
        Assert.Contains("Id", propertyNames);
        Assert.Contains("Name", propertyNames);
        Assert.Contains("PromptBody", propertyNames);
        Assert.Contains("TaskNumber", propertyNames);
        Assert.Contains("Type", propertyNames);
        Assert.Contains("ReferenceUserStory", propertyNames);
        Assert.Contains("UserStoryId", propertyNames);
        Assert.Contains("ApprovedByUserUtc", propertyNames);
        Assert.Contains("Paused", propertyNames);
        Assert.Contains("RequestedByAiUtc", propertyNames);
        Assert.Contains("FinishedByAiUtc", propertyNames);
        Assert.Contains("CreatedOrUpdatedUtc", propertyNames);
        Assert.Contains("PromptAppendThink", propertyNames);
        Assert.Contains("PromptAppendThinkHard", propertyNames);
        Assert.Contains("PromptAppendDoNotChange", propertyNames);
        Assert.Contains("UserStoryNumber", propertyNames);
        
        // Ensure there are no "UserStory" navigation properties
        Assert.DoesNotContain("UserStory", propertyNames);
    }
    
    [Fact]
    public async Task TaskNumber_Should_Increment_For_Each_New_Task()
    {
        // Arrange - Create project and user story
        var projectName = "TaskNumber Increment Test Project";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        var userStoryName = "TaskNumber Test UserStory";
        var createUserStoryRequest = new CreateUserStoryRequest(userStoryName, projectId);
        var createUserStoryResult = await _createUserStoryHandler.Handle(createUserStoryRequest, CancellationToken.None);
        
        Assert.True(createUserStoryResult.IsSuccess);
        var userStoryId = createUserStoryResult.Value;
        
        // Create first task
        var task1Name = "TaskNumber Test Task 1";
        var promptBody1 = "Test prompt for task 1";
        var createTask1Request = new CreateTaskRequest(task1Name, promptBody1, userStoryId);
        var createTask1Result = await _createTaskHandler.Handle(createTask1Request, CancellationToken.None);
        
        Assert.True(createTask1Result.IsSuccess);
        var task1Id = createTask1Result.Value;
        
        // Create second task
        var task2Name = "TaskNumber Test Task 2";
        var promptBody2 = "Test prompt for task 2";
        var createTask2Request = new CreateTaskRequest(task2Name, promptBody2, userStoryId);
        var createTask2Result = await _createTaskHandler.Handle(createTask2Request, CancellationToken.None);
        
        Assert.True(createTask2Result.IsSuccess);
        var task2Id = createTask2Result.Value;
        
        // Create third task
        var task3Name = "TaskNumber Test Task 3";
        var promptBody3 = "Test prompt for task 3";
        var createTask3Request = new CreateTaskRequest(task3Name, promptBody3, userStoryId);
        var createTask3Result = await _createTaskHandler.Handle(createTask3Request, CancellationToken.None);
        
        Assert.True(createTask3Result.IsSuccess);
        var task3Id = createTask3Result.Value;
        
        // Act - Get all three tasks
        var getTask1Request = new GetTaskByIdRequest(task1Id);
        var getTask2Request = new GetTaskByIdRequest(task2Id);
        var getTask3Request = new GetTaskByIdRequest(task3Id);
        
        var getTask1Result = await _getTaskByIdHandler.Handle(getTask1Request, CancellationToken.None);
        var getTask2Result = await _getTaskByIdHandler.Handle(getTask2Request, CancellationToken.None);
        var getTask3Result = await _getTaskByIdHandler.Handle(getTask3Request, CancellationToken.None);
        
        // Assert - Verify all results are successful
        Assert.True(getTask1Result.IsSuccess);
        Assert.True(getTask2Result.IsSuccess);
        Assert.True(getTask3Result.IsSuccess);
        
        var taskModel1 = getTask1Result.Value;
        var taskModel2 = getTask2Result.Value;
        var taskModel3 = getTask3Result.Value;
        
        // Verify task numbers are sequential and start at 1
        Assert.Equal(1, taskModel1.TaskNumber);
        Assert.Equal(2, taskModel2.TaskNumber);
        Assert.Equal(3, taskModel3.TaskNumber);
    }
    
    [Fact]
    public async Task TaskNumber_Should_Be_Project_Specific()
    {
        // Arrange - Create two different projects
        var project1Name = "TaskNumber Project 1";
        var project2Name = "TaskNumber Project 2";
        
        var createProject1Request = new CreateProjectRequest(project1Name);
        var createProject2Request = new CreateProjectRequest(project2Name);
        
        var createProject1Result = await _createProjectHandler.Handle(createProject1Request, CancellationToken.None);
        var createProject2Result = await _createProjectHandler.Handle(createProject2Request, CancellationToken.None);
        
        Assert.True(createProject1Result.IsSuccess);
        Assert.True(createProject2Result.IsSuccess);
        
        var project1Id = createProject1Result.Value;
        var project2Id = createProject2Result.Value;
        
        // Create user stories for each project
        var project1UserStoryName = "Project1 UserStory for Tasks";
        var project2UserStoryName = "Project2 UserStory for Tasks";
        
        var createProject1UserStoryRequest = new CreateUserStoryRequest(project1UserStoryName, project1Id);
        var createProject2UserStoryRequest = new CreateUserStoryRequest(project2UserStoryName, project2Id);
        
        var createProject1UserStoryResult = await _createUserStoryHandler.Handle(createProject1UserStoryRequest, CancellationToken.None);
        var createProject2UserStoryResult = await _createUserStoryHandler.Handle(createProject2UserStoryRequest, CancellationToken.None);
        
        Assert.True(createProject1UserStoryResult.IsSuccess);
        Assert.True(createProject2UserStoryResult.IsSuccess);
        
        var project1UserStoryId = createProject1UserStoryResult.Value;
        var project2UserStoryId = createProject2UserStoryResult.Value;
        
        // Create tasks for the first project
        var project1Task1Name = "Project1 Task 1";
        var project1Task2Name = "Project1 Task 2";
        
        var createProject1Task1Request = new CreateTaskRequest(project1Task1Name, "Project1 prompt 1", project1UserStoryId);
        var createProject1Task2Request = new CreateTaskRequest(project1Task2Name, "Project1 prompt 2", project1UserStoryId);
        
        var createProject1Task1Result = await _createTaskHandler.Handle(createProject1Task1Request, CancellationToken.None);
        var createProject1Task2Result = await _createTaskHandler.Handle(createProject1Task2Request, CancellationToken.None);
        
        Assert.True(createProject1Task1Result.IsSuccess);
        Assert.True(createProject1Task2Result.IsSuccess);
        
        var project1Task1Id = createProject1Task1Result.Value;
        var project1Task2Id = createProject1Task2Result.Value;
        
        // Create tasks for the second project
        var project2Task1Name = "Project2 Task 1";
        var project2Task2Name = "Project2 Task 2";
        
        var createProject2Task1Request = new CreateTaskRequest(project2Task1Name, "Project2 prompt 1", project2UserStoryId);
        var createProject2Task2Request = new CreateTaskRequest(project2Task2Name, "Project2 prompt 2", project2UserStoryId);
        
        var createProject2Task1Result = await _createTaskHandler.Handle(createProject2Task1Request, CancellationToken.None);
        var createProject2Task2Result = await _createTaskHandler.Handle(createProject2Task2Request, CancellationToken.None);
        
        Assert.True(createProject2Task1Result.IsSuccess);
        Assert.True(createProject2Task2Result.IsSuccess);
        
        var project2Task1Id = createProject2Task1Result.Value;
        var project2Task2Id = createProject2Task2Result.Value;
        
        // Act - Get all tasks
        var getProject1Task1Request = new GetTaskByIdRequest(project1Task1Id);
        var getProject1Task2Request = new GetTaskByIdRequest(project1Task2Id);
        var getProject2Task1Request = new GetTaskByIdRequest(project2Task1Id);
        var getProject2Task2Request = new GetTaskByIdRequest(project2Task2Id);
        
        var getProject1Task1Result = await _getTaskByIdHandler.Handle(getProject1Task1Request, CancellationToken.None);
        var getProject1Task2Result = await _getTaskByIdHandler.Handle(getProject1Task2Request, CancellationToken.None);
        var getProject2Task1Result = await _getTaskByIdHandler.Handle(getProject2Task1Request, CancellationToken.None);
        var getProject2Task2Result = await _getTaskByIdHandler.Handle(getProject2Task2Request, CancellationToken.None);
        
        // Assert - Verify all results are successful
        Assert.True(getProject1Task1Result.IsSuccess);
        Assert.True(getProject1Task2Result.IsSuccess);
        Assert.True(getProject2Task1Result.IsSuccess);
        Assert.True(getProject2Task2Result.IsSuccess);
        
        var project1Task1Model = getProject1Task1Result.Value;
        var project1Task2Model = getProject1Task2Result.Value;
        var project2Task1Model = getProject2Task1Result.Value;
        var project2Task2Model = getProject2Task2Result.Value;
        
        // Verify task numbers are sequential within each project and both start at 1
        Assert.Equal(1, project1Task1Model.TaskNumber);
        Assert.Equal(2, project1Task2Model.TaskNumber);
        Assert.Equal(1, project2Task1Model.TaskNumber);
        Assert.Equal(2, project2Task2Model.TaskNumber);
    }
    
    [Fact]
    public async Task TaskNumber_Should_Be_Shared_Across_UserStories_In_Same_Project()
    {
        // Arrange - Create a project
        var projectName = "TaskNumber Shared Project";
        var createProjectRequest = new CreateProjectRequest(projectName);
        var createProjectResult = await _createProjectHandler.Handle(createProjectRequest, CancellationToken.None);
        
        Assert.True(createProjectResult.IsSuccess);
        var projectId = createProjectResult.Value;
        
        // Create two user stories in the same project
        var userStory1Name = "TaskNumber Shared UserStory 1";
        var userStory2Name = "TaskNumber Shared UserStory 2";
        
        var createUserStory1Request = new CreateUserStoryRequest(userStory1Name, projectId);
        var createUserStory2Request = new CreateUserStoryRequest(userStory2Name, projectId);
        
        var createUserStory1Result = await _createUserStoryHandler.Handle(createUserStory1Request, CancellationToken.None);
        var createUserStory2Result = await _createUserStoryHandler.Handle(createUserStory2Request, CancellationToken.None);
        
        Assert.True(createUserStory1Result.IsSuccess);
        Assert.True(createUserStory2Result.IsSuccess);
        
        var userStory1Id = createUserStory1Result.Value;
        var userStory2Id = createUserStory2Result.Value;
        
        // Create tasks for the first user story
        var userStory1Task1Name = "US1 Task 1";
        var userStory1Task2Name = "US1 Task 2";
        
        var createUserStory1Task1Request = new CreateTaskRequest(userStory1Task1Name, "US1 prompt 1", userStory1Id);
        var createUserStory1Task2Request = new CreateTaskRequest(userStory1Task2Name, "US1 prompt 2", userStory1Id);
        
        var createUserStory1Task1Result = await _createTaskHandler.Handle(createUserStory1Task1Request, CancellationToken.None);
        var createUserStory1Task2Result = await _createTaskHandler.Handle(createUserStory1Task2Request, CancellationToken.None);
        
        Assert.True(createUserStory1Task1Result.IsSuccess);
        Assert.True(createUserStory1Task2Result.IsSuccess);
        
        var userStory1Task1Id = createUserStory1Task1Result.Value;
        var userStory1Task2Id = createUserStory1Task2Result.Value;
        
        // Create tasks for the second user story
        var userStory2Task1Name = "US2 Task 1";
        var userStory2Task2Name = "US2 Task 2";
        
        var createUserStory2Task1Request = new CreateTaskRequest(userStory2Task1Name, "US2 prompt 1", userStory2Id);
        var createUserStory2Task2Request = new CreateTaskRequest(userStory2Task2Name, "US2 prompt 2", userStory2Id);
        
        var createUserStory2Task1Result = await _createTaskHandler.Handle(createUserStory2Task1Request, CancellationToken.None);
        var createUserStory2Task2Result = await _createTaskHandler.Handle(createUserStory2Task2Request, CancellationToken.None);
        
        Assert.True(createUserStory2Task1Result.IsSuccess);
        Assert.True(createUserStory2Task2Result.IsSuccess);
        
        var userStory2Task1Id = createUserStory2Task1Result.Value;
        var userStory2Task2Id = createUserStory2Task2Result.Value;
        
        // Act - Get all tasks
        var getUserStory1Task1Request = new GetTaskByIdRequest(userStory1Task1Id);
        var getUserStory1Task2Request = new GetTaskByIdRequest(userStory1Task2Id);
        var getUserStory2Task1Request = new GetTaskByIdRequest(userStory2Task1Id);
        var getUserStory2Task2Request = new GetTaskByIdRequest(userStory2Task2Id);
        
        var getUserStory1Task1Result = await _getTaskByIdHandler.Handle(getUserStory1Task1Request, CancellationToken.None);
        var getUserStory1Task2Result = await _getTaskByIdHandler.Handle(getUserStory1Task2Request, CancellationToken.None);
        var getUserStory2Task1Result = await _getTaskByIdHandler.Handle(getUserStory2Task1Request, CancellationToken.None);
        var getUserStory2Task2Result = await _getTaskByIdHandler.Handle(getUserStory2Task2Request, CancellationToken.None);
        
        // Assert - Verify all results are successful
        Assert.True(getUserStory1Task1Result.IsSuccess);
        Assert.True(getUserStory1Task2Result.IsSuccess);
        Assert.True(getUserStory2Task1Result.IsSuccess);
        Assert.True(getUserStory2Task2Result.IsSuccess);
        
        var userStory1Task1Model = getUserStory1Task1Result.Value;
        var userStory1Task2Model = getUserStory1Task2Result.Value;
        var userStory2Task1Model = getUserStory2Task1Result.Value;
        var userStory2Task2Model = getUserStory2Task2Result.Value;
        
        // Verify task numbers are continuous across user stories within the same project
        Assert.Equal(1, userStory1Task1Model.TaskNumber);
        Assert.Equal(2, userStory1Task2Model.TaskNumber);
        Assert.Equal(3, userStory2Task1Model.TaskNumber);
        Assert.Equal(4, userStory2Task2Model.TaskNumber);
    }
}