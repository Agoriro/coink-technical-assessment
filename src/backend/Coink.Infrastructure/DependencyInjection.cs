using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Coink.Infrastructure;

/// <summary>
/// Registers PostgreSQL and other technical infrastructure owned by this layer.
/// </summary>
public static class DependencyInjection
{
    private const string PostgresConnectionName = "Postgres";

    /// <summary>
    /// Adds infrastructure services using environment-aware application configuration.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="configuration">Configuration containing the PostgreSQL connection string.</param>
    /// <returns>The same collection for composition chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required PostgreSQL connection string is absent.
    /// </exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string? connectionString = configuration.GetConnectionString(PostgresConnectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{PostgresConnectionName}' is required.");
        }

        _ = services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        return services;
    }
}
