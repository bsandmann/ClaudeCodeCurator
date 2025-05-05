using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.CreateTask;
using ClaudeCodeCurator.Commands.CreateUserStory;
using ClaudeCodeCurator.Commands.GetProjectById;
using ClaudeCodeCurator.Commands.GetTaskById;
using ClaudeCodeCurator.Commands.GetUserStoryById;
using ClaudeCodeCurator.Commands.MoveTaskInProjectOrder;
using ClaudeCodeCurator.Commands.RemoveProject;
using ClaudeCodeCurator.Commands.RemoveTask;
using ClaudeCodeCurator.Commands.RemoveUserStory;
using ClaudeCodeCurator.Commands.SetAiTaskFinishState;
using ClaudeCodeCurator.Commands.SetAiTaskRequestState;
using ClaudeCodeCurator.Commands.SetTaskPause;
using ClaudeCodeCurator.Commands.SetUserTaskApproval;
using ClaudeCodeCurator.Commands.UpdateProject;
using ClaudeCodeCurator.Commands.UpdateTask;
using ClaudeCodeCurator.Commands.UpdateUserStory;
using ClaudeCodeCurator.Common;
using FluentResults;
using LazyCache;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace ClaudeCodeCurator.Tests;

[Collection("TransactionalTests")]
public partial class IntegrationTests : IDisposable
{
    private TransactionalTestDatabaseFixture Fixture { get; }
    readonly IAppCache _mockedCache;
    readonly IOptions<AppSettings> _appSettingsOptions;
    private readonly DataContext _context;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly IServiceProvider _serviceProviderMock;
    private readonly CreateProjectHandler _createProjectHandler;
    private readonly UpdateProjectHandler _updateProjectHandler;
    private readonly CreateUserStoryHandler _createUserStoryHandler;
    private readonly UpdateUserStoryHandler _updateUserStoryHandler;
    private readonly CreateTaskHandler _createTaskHandler;
    private readonly UpdateTaskHandler _updateTaskHandler;
    private readonly GetTaskByIdHandler _getTaskByIdHandler;
    private readonly GetUserStoryByIdHandler _getUserStoryByIdHandler;
    private readonly GetProjectByIdHandler _getProjectByIdHandler;
    private readonly RemoveTaskHandler _removeTaskHandler;
    private readonly RemoveUserStoryHandler _removeUserStoryHandler;
    private readonly RemoveProjectHandler _removeProjectHandler;
    private readonly SetUserTaskApprovalHandler _setUserTaskApprovalHandler;
    private readonly SetAiTaskRequestStateHandler _setAiTaskRequestStateHandler;
    private readonly SetAiTaskFinishStateHandler _setAiTaskFinishStateHandler;
    private readonly MoveTaskInProjectOrderHandler _moveTaskInProjectOrderHandler;
    private readonly IServiceProvider _serviceProvider;
    
    public IntegrationTests(TransactionalTestDatabaseFixture fixture)
    {
        this.Fixture = fixture;
        this._context = this.Fixture.CreateContext();
        this._appSettingsOptions = Options.Create(new AppSettings()
        {

        });
        
        // Set up mocks
        this._mediatorMock = new Mock<IMediator>();
        this._mockedCache = LazyCache.Testing.Moq.Create.MockedCachingService();


        // Setup a service collection and service provider for DI testing
        var services = new ServiceCollection();
        services.AddSingleton(_context);
        services.AddTransient<SetTaskPauseHandler>(provider => 
            new SetTaskPauseHandler(_serviceScopeFactoryMock.Object));
            
        _serviceProvider = services.BuildServiceProvider();
            
        // Create a mock service provider that returns the test context
        _serviceProviderMock = Mock.Of<IServiceProvider>(sp => 
            sp.GetService(typeof(DataContext)) == _context);
            
        // Create a mock service scope that returns our mocked service provider
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock);
        
        // Create a mock service scope factory that returns our mocked service scope
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeFactoryMock
            .Setup(f => f.CreateScope())
            .Returns(_serviceScopeMock.Object);

        // Create handlers with the real implementations
        this._createProjectHandler = new CreateProjectHandler(_serviceScopeFactoryMock.Object);
        this._updateProjectHandler = new UpdateProjectHandler(_serviceScopeFactoryMock.Object);
        this._createUserStoryHandler = new CreateUserStoryHandler(_serviceScopeFactoryMock.Object);
        this._updateUserStoryHandler = new UpdateUserStoryHandler(_serviceScopeFactoryMock.Object);
        this._createTaskHandler = new CreateTaskHandler(_serviceScopeFactoryMock.Object);
        this._updateTaskHandler = new UpdateTaskHandler(_serviceScopeFactoryMock.Object);
        this._getTaskByIdHandler = new GetTaskByIdHandler(_serviceScopeFactoryMock.Object);
        this._getUserStoryByIdHandler = new GetUserStoryByIdHandler(_serviceScopeFactoryMock.Object);
        this._getProjectByIdHandler = new GetProjectByIdHandler(_serviceScopeFactoryMock.Object);
        this._removeTaskHandler = new RemoveTaskHandler(_serviceScopeFactoryMock.Object);
        this._removeUserStoryHandler = new RemoveUserStoryHandler(_serviceScopeFactoryMock.Object);
        this._removeProjectHandler = new RemoveProjectHandler(_serviceScopeFactoryMock.Object);
        this._setUserTaskApprovalHandler = new SetUserTaskApprovalHandler(_serviceScopeFactoryMock.Object);
        this._setAiTaskRequestStateHandler = new SetAiTaskRequestStateHandler(_serviceScopeFactoryMock.Object);
        this._setAiTaskFinishStateHandler = new SetAiTaskFinishStateHandler(_serviceScopeFactoryMock.Object);
        this._moveTaskInProjectOrderHandler = new MoveTaskInProjectOrderHandler(_serviceScopeFactoryMock.Object);

        // Initialize handlers with the mocked service scope factory
    }

    public void Dispose()
        => this.Fixture.Cleanup();
        

}