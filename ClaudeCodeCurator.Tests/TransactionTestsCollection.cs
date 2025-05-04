namespace ClaudeCodeCurator.Tests;

using OpenPrismNode.Core.IntegrationTests;
using Xunit;

[CollectionDefinition("TransactionalTests", DisableParallelization = true)]
public class TransactionalTestsCollection : ICollectionFixture<TransactionalTestDatabaseFixture>
{
}