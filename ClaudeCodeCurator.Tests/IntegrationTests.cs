using ClaudeCodeCurator.Commands.CreateProject;
using ClaudeCodeCurator.Commands.UpdateProject;
using ClaudeCodeCurator.Common;
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


        this._createProjectHandler = new CreateProjectHandler(_serviceScopeFactoryMock.Object);
        this._updateProjectHandler = new UpdateProjectHandler(_serviceScopeFactoryMock.Object);

        // Initialize handlers with the mocked service scope factory
    }

    public void Dispose()
        => this.Fixture.Cleanup();
        

}