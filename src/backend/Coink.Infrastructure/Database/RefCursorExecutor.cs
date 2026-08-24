using Dapper;
using Npgsql;

namespace Coink.Infrastructure.Database;

internal sealed class RefCursorExecutor(NpgsqlDataSource dataSource)
{
    internal async Task<IReadOnlyList<T>> QueryAsync<T>(
        string callCommandText,
        string fetchCommandText,
        object? parameters,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(
            cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(
            cancellationToken);

        CommandDefinition callCommand = new(
            callCommandText,
            parameters,
            transaction,
            cancellationToken: cancellationToken);
        _ = await connection.ExecuteAsync(callCommand);

        CommandDefinition fetchCommand = new(
            fetchCommandText,
            transaction: transaction,
            cancellationToken: cancellationToken);
        IEnumerable<T> result = await connection.QueryAsync<T>(fetchCommand);
        List<T> rows = [.. result];

        await transaction.CommitAsync(cancellationToken);
        return rows;
    }
}
