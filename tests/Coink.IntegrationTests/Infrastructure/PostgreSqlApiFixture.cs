using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Coink.IntegrationTests.Infrastructure;

public sealed partial class PostgreSqlApiFixture : IAsyncLifetime
{
    private const string PostgreSqlImage =
        "postgres:16-alpine@sha256:cf78e76683b9ca8c5733cbbdce6c9262b45b6767934dd0a95e671f9a0fc20685";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(PostgreSqlImage)
        .WithDatabase("coink_tests")
        .WithUsername("coink_tests")
        .WithPassword("integration-only-password")
        .WithCleanUp(true)
        .Build();
    private WebApplicationFactory<Program>? _application;

    public HttpClient Client { get; private set; } = null!;

    public string ConnectionString =>
        $"{_postgres.GetConnectionString()};GSS Encryption Mode=Disable";

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await InitializeDatabaseAsync();

        _application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            _ = builder.UseEnvironment("Testing");
            _ = builder.ConfigureTestServices(services =>
            {
                _ = services.RemoveAll<NpgsqlDataSource>();
                _ = services.AddSingleton(_ => NpgsqlDataSource.Create(ConnectionString));
            });
            _ = builder.ConfigureLogging(logging => logging.ClearProviders());
        });
        Client = _application.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_application is not null)
        {
            await _application.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    public async Task ResetUsersAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "truncate table app.users restart identity;",
            connection);
        _ = await command.ExecuteNonQueryAsync();
    }

    public async Task<long> CountUsersAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("select count(*) from app.users;", connection);
        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task InitializeDatabaseAsync()
    {
        string repositoryRoot = FindRepositoryRoot();
        string initializerPath = Path.Combine(
            repositoryRoot,
            "database",
            "init",
            "001_initialize.sql");
        string initializer = await File.ReadAllTextAsync(initializerPath);
        MatchCollection includes = IncludeExpression().Matches(initializer);
        Assert.NotEmpty(includes);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        foreach (Match include in includes)
        {
            string relativePath = include.Groups[1].Value.Trim();
            string scriptPath = Path.GetFullPath(
                Path.Combine(Path.GetDirectoryName(initializerPath)!, relativePath));
            string databaseRoot = Path.GetFullPath(Path.Combine(repositoryRoot, "database"));
            if (!scriptPath.StartsWith(databaseRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Database initializer references an unsafe path.");
            }

            string sql = await File.ReadAllTextAsync(scriptPath);
            await using var command = new NpgsqlCommand(sql, connection, transaction)
            {
                CommandTimeout = 120,
            };
            _ = await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "COINK.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root.");
    }

    [GeneratedRegex(@"^\\ir\s+(.+?)\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex IncludeExpression();
}
