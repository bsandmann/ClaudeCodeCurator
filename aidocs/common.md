# CLAUDE.md - C# Coding Guidelines

## Build & Test Commands

- Build project: `dotnet build`
- Build specific project with UI (startup project): `dotnet build ClaudeCodeCurator/ClaudeCodeCurator.csproj`
- Run all tests: `dotnet test`
- Run specific test project: `dotnet test ClaudeCodeCurator/ClaudeCodeCurator.Tests.csproj`

## Code Style Guidelines

- **Naming**: Use PascalCase for classes, methods, properties, enums; camelCase with underscore prefix for private
  fields
- **Imports**: Order by System namespaces first, then project namespaces; sort alphabetically within groups
- **Types**: Use explicit types; prefer `var` only when type is obvious or for complex LINQ expressions
- **Error Handling**: Use FluentResults pattern with Result<T> for method returns; include descriptive error messages.
  Don't use exceptions for control flow, only if absolutely necessary. For example, if a file is not found, return a
  Result with an error message instead of throwing an exception. Use Result.Ok() for successful operations and
  Result.Fail("some message") for errors. Try to avoid explicit types for Result<T> unless necessary e.g. avoid "return
  Result.Ok<string>(someMessage)" and just "return someMessage" or the implicit "return Result.Ok(someMessage)". In the
  end this means all method that can fail in any way should return a Result<T>.
- **Organization**: Follow CQRS pattern with Commands/Handlers; In the main project there should be a "Commands" folder.
  Inside that, there should be folders for each command e.g. "CreateUser". Inside that there should be two files "
  CreateUserRequest.cs" and "CreateUserHandler.cs". The request file should contain the request class and the handler
  file should contain the handler class. The handler class should implement the IRequestHandler interface from MediatR.
  The request class should implement the IRequest interface from MediatR. The request class should also contain a
  constructor that takes in all the required parameters for the command. The handler class should contain a constructor
  that takes in all the required dependencies for the command. The handler should implement IRequestHandler.
- **Async**: Use async/await consistently; avoid blocking calls and sync-over-async patterns
- **Comments**: Use XML docs for public APIs; inline comments for implementation details; avoid regions
- **Brackets**: Use braces for all control structures, even single-line statements. Do not use IF-statements without
  braces. For example, do not use "if (condition) DoSomething();" instead use "if (condition) { DoSomething(); }".
- **Nullability**: Use nullable reference types; default to non-nullable; use `?` for nullable types
- **Constructors**: Use constructors for setting properties; Avoid using public setters for properties. For example, do
  not use "public string Name { get; set; }" instead use "public string Name { get; }" and set the value in the
  constructor.
- **Dates and Locale**: Use UTC for all date/time values in the backend.

## Common libraries

- **FluentResults**: Use for error handling and result management
- **MediatR**: Use for CQRS pattern implementation.
- **Sentry**: Use for error tracking and monitoring
- **Moq**: Use for unit testing; create mocks in test classes
- **Humanizer**: Use for string manipulation and formatting

## Data Access with Postgres Server (for backend projects)

- **Entity Framework Core**: Use the most recent stable version for Entity Framework for PostgreSQL.
- **Repository**: The database related operations should all be captured as commands e.g. "CreatePerson"-command, "
  UpdatePerson"-command
- **DbContext**: Use a single DbContext called "DataContext". There should be a "DataContext.cs" file. Do not inject
  DbContext directly, but use `IServiceScopeFactory` instead and get the context this way:
  `using var scope = _serviceScopeFactory.CreateScope(); var context = scope.ServiceProvider.GetRequiredService<DataContext>();`
- **Migrations**: Use EF Core migrations for database schema changes; name migrations descriptively. Run
  `dotnet ef migrations add <MigrationName>` to create a migration and `dotnet ef database update` to apply it. Use
  `dotnet ef migrations remove` to remove the last migration.
- **Connection Strings**: Store connection strings in `appsettings.json`; use `IConfiguration` to access them. Use the "
  DefaultConnection" key for the main connection string.
- **Entities**: Use POCO classes for entities; follow naming conventions (e.g. "PersonEntity" for a person entity). Use
  the `Fluent API` to configure the entities in the `OnModelCreating` method of the `DataContext` class. Avoid using
  Attributes for configuration. Be explicit in the configuration.
- If GUIDs are used as primary keys, consider setting using a SequentialGuidValueGenerator.
- Save dates in the UTC format.
- Try to optimize properties for performance and size, eg. set MaxLength, Unicode, etc.
- Avoid Tracking instances of entities unless necessary. Use `AsNoTracking()` for read-only queries. For example, use
  `_context.Persons.AsNoTracking().ToList()` instead of `_context.Persons.ToList()`.

## Application Setup

- **Program.cs**: Do NOT use the new minimal hosting model; configure services and middleware in a single `Program.cs`
  file
- **Dependency Injection**: Use built-in DI container; register services in `Program.cs` using `AddScoped`,
  `AddTransient`, or `AddSingleton` as appropriate
- **Configuration**: Use `IConfiguration` for app settings; bind to strongly-typed classes for complex settings. Create
  a `AppSettings.cs` and bind all common settings to that class. Avoid hard-coded values in the codebase. Use
  `IOptions<AppSettings>` to access the settings in the codebase.
- **Logging**: Use built-in logging framework; configure logging in `Program.cs`; Log everything to the console in
  development. Only log warning and errors in production to the console.
- **Migrations**: Use EF Core migrations for database schema changes; apply migrations in `Program.cs` on startup
- **Sentry**: Use Sentry for error tracking and tracing in the production environment (not in development). Configure in
  `Program.cs` like this `builder.WebHost.UseSentry();`. The
  rest of the Sentry configuration is in the `appsettings.json` file and should like this:
- ```json
    "Sentry": {
    "Dsn": "provided later when deployed",
    "SendDefaultPii": true,
    "MaxRequestBodySize": "Always",
    "MinimumBreadcrumbLevel": "Debug",
    "MinimumEventLevel": "Warning",
    "AttachStackTrace": true,
    "DiagnosticLevel": "Warning",
    "TracesSampleRate": 1.0
  }
  ```
-

## Blazor Server Interactive mode (for frontend projects)

- **Interactive Mode**: Use Blazor Server for interactive mode; create a separate project for the Blazor Server app
- **Components**: Use Blazor components for UI; follow the component naming convention of "ComponentName.razor".
- **Routing**: Use `@page` directive for routing; define routes in the component files. Normal components which are not
  pages should not have the `@page` directive.
- **State Management**: Use `CascadingValue` for state management; avoid using static classes for state.
- **RenderMode**: Use `@rendermode @(new InteractiveServerRenderMode(prerender: false))` on the components when they
  contain interactive elements
- **Localization**: User German as the language for the displayed UI texts, dates (format dd.MM.yyyy) and numbers
  (formatexamples: 1.000 (one thousand) and 1,23 (decimal)) and 24h format for time. Do not use German culture for the
  backend or in the Frontend code - only for display purposes.
- **Database access**: Never ever use the DbContext directly in the Blazor components. Always use a command through MediatR. 

## Tailwind (for frontend projects)

- **Tailwind CSS**: Use Tailwind CSS for styling; Create a tailwind.config.js file in the root of the project. In the
  package.json reference `tailwindcss` as `devDependencies` with a current version. Create a TailwindStyles folder which
  should contain the app.css generated by tailwind. You can assume that this is running in the background:
  `npx tailwindcss -i ./TailwindStyles/app.css -o ./wwwroot/app.css --watch`
- **Remove other CSS**: Remove all other CSS files from the project. Do not use any other CSS framework (e.g.
  bootstrap).
- **Icons**: For icons use the material design icons like this:   <span class="material-symbols-outlined">close</span>


## Testing

- **Unit Tests**: Use xUnit for unit testing; create a separate test project for each main project
- **Test Naming**: Use descriptive names for test methods; follow the pattern
  `MethodName_StateUnderTest_ExpectedBehavior`
- **Test Structure**: Use Arrange-Act-Assert pattern; separate setup, execution, and verification steps
- **Assertions**: Use FluentAssertions for assertions; avoid using `Assert.Equal` or `Assert.True` directly. Do use library version 7.2.0. Do not use any other version.
- **Mocking**: Use Moq for mocking dependencies; create mocks in the test class setup.
- **Cache**: When LazyCache is used, never test the cache. Always mock the test in a way that the cache is not used, so that we test the actual implementation. Use 'this._mockedCache = LazyCache.Testing.Moq.Create.MockedCachingService();' to mock the cache - MockedCachingService() is part of the library LazyCache.Testing.Moq.
- **Commands inside commands**: When testing a command, which calls another command, setup a mock of the mediator which refers the call to the inner command e.g. `this._mediatorMock = new Mock<IMediator>();` and `this._mediatorMock.Setup(p => p.Send(It.IsAny<InnerCommandRequest>(), It.IsAny<CancellationToken>())).Returns(async (InnerCommandRequest request, CancellationToken token) => await this._innerCommandHandler.Handle(request, token));`.
- **Logger**: If a logger is used inside the testable class, do not test the logger or the output of the logger. Mock the logger and ignore it for testing purposes.
- **Test Data**: Use a separate Postgres Server database for integration tests. DO NOT use an in-memory database; use a test database connection string in
  `appsettings.Test.json`. Run every database related test against that test-database. In the relevant Test project have
  a file called "TransactionalTestDatabaseFixture.cs" to setup the tests.
```

Also have a `TransactionTestsCollection.cs` file with this content:

```csharp
[CollectionDefinition("TransactionalTests", DisableParallelization = true)]
public class TransactionalTestsCollection : ICollectionFixture<TransactionalTestDatabaseFixture>
{
}
```

Then use `[Collection("TransactionalTests")]` as the attribute for all database related tests.

Maintain consistency with existing patterns across the codebase.
