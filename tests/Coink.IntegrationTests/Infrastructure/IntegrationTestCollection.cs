namespace Coink.IntegrationTests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestSuite : ICollectionFixture<PostgreSqlApiFixture>
{
    public const string Name = "PostgreSQL API integration";
}
