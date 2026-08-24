using Coink.IntegrationTests.Infrastructure;
using Npgsql;

namespace Coink.IntegrationTests.Database;

[Collection(IntegrationTestSuite.Name)]
public sealed class DatabaseIntegrityTests(PostgreSqlApiFixture fixture)
{
    [Fact]
    public async Task GeographicConstraintRejectsMismatchedHierarchy()
    {
        await fixture.ResetUsersAsync();
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            insert into app.users (
                name, phone, country_id, department_id, municipality_id, address
            )
            values ('Constraint Test', '3001234567', 170, 5, 11001, 'Calle 1');
            """,
            connection);

        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(
            command.ExecuteNonQueryAsync);

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, exception.SqlState);
        Assert.Equal(0, await fixture.CountUsersAsync());
    }

    [Fact]
    public async Task ProcedureWriteCanBeRolledBackAtomically()
    {
        await fixture.ResetUsersAsync();
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();
        await using var command = new NpgsqlCommand(
            """
            call app.create_user(
                'Rollback Test',
                '3007654321',
                170::smallint,
                5::smallint,
                5001,
                'Calle 2',
                'rollback_result');
            """,
            connection,
            transaction);

        _ = await command.ExecuteNonQueryAsync();
        await transaction.RollbackAsync();

        Assert.Equal(0, await fixture.CountUsersAsync());
    }
}
