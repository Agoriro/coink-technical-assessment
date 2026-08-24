using Coink.Application.Users;
using Coink.Infrastructure.Database;
using Npgsql;

namespace Coink.Infrastructure.Users;

internal sealed class UserRepository(RefCursorExecutor cursorExecutor) : IUserRepository
{
    private const string ForeignKeyViolationSqlState = "23503";
    private const string CreateUserCall = """
        call app.create_user(
            @Name,
            @Phone,
            @CountryId,
            @DepartmentId,
            @MunicipalityId,
            @Address,
            'api_create_user');
        """;
    private const string FetchCreatedUser = "fetch all from api_create_user;";
    private const string GetUserCall = "call app.get_user(@UserId, 'api_get_user');";
    private const string FetchUser = "fetch all from api_get_user;";
    private const string ListUsersCall =
        "call app.list_users(@Page, @PageSize, @Search, 'api_list_users');";
    private const string FetchUsers = "fetch all from api_list_users;";
    private const string UpdateUserCall = """
        call app.update_user(
            @UserId,
            @Name,
            @Phone,
            @CountryId,
            @DepartmentId,
            @MunicipalityId,
            @Address,
            'api_update_user');
        """;
    private const string FetchUpdatedUser = "fetch all from api_update_user;";
    private const string DeleteUserCall =
        "call app.delete_user(@UserId, 'api_delete_user');";
    private const string FetchDeletedUser = "fetch all from api_delete_user;";

    public async Task<UserPersistenceResult> CreateAsync(
        UserInput input,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<UserRow> rows = await cursorExecutor.QueryAsync<UserRow>(
                CreateUserCall,
                FetchCreatedUser,
                ToParameters(input),
                cancellationToken);

            UserReference user = MapRequiredSingle(rows, "create_user");
            return new UserPersistenceResult(UserPersistenceStatus.Success, user);
        }
        catch (PostgresException exception)
            when (exception.SqlState == ForeignKeyViolationSqlState)
        {
            return new UserPersistenceResult(UserPersistenceStatus.InvalidGeography);
        }
    }

    public async Task<UserReference?> GetAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<UserRow> rows = await cursorExecutor.QueryAsync<UserRow>(
            GetUserCall,
            FetchUser,
            new { UserId = userId },
            cancellationToken);

        return rows.Count switch
        {
            0 => null,
            1 => Map(rows[0]),
            _ => throw new InvalidOperationException("get_user returned more than one row."),
        };
    }

    public async Task<UserPage> ListAsync(
        UserListQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<UserListRow> rows = await cursorExecutor.QueryAsync<UserListRow>(
            ListUsersCall,
            FetchUsers,
            new
            {
                query.Page,
                query.PageSize,
                query.Search,
            },
            cancellationToken);

        if (rows.Count == 0)
        {
            throw new InvalidOperationException("list_users returned no metadata row.");
        }

        UserReference[] users =
        [
            .. rows
                .Where(static row => row.Id.HasValue)
                .Select(Map),
        ];

        return new UserPage(users, query.Page, query.PageSize, rows[0].TotalCount);
    }

    public async Task<UserPersistenceResult> UpdateAsync(
        long userId,
        UserInput input,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<UserRow> rows = await cursorExecutor.QueryAsync<UserRow>(
                UpdateUserCall,
                FetchUpdatedUser,
                ToParameters(input, userId),
                cancellationToken);

            return rows.Count switch
            {
                0 => new UserPersistenceResult(UserPersistenceStatus.NotFound),
                1 => new UserPersistenceResult(UserPersistenceStatus.Success, Map(rows[0])),
                _ => throw new InvalidOperationException(
                    "update_user returned more than one row."),
            };
        }
        catch (PostgresException exception)
            when (exception.SqlState == ForeignKeyViolationSqlState)
        {
            return new UserPersistenceResult(UserPersistenceStatus.InvalidGeography);
        }
    }

    public async Task<long?> DeleteAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<DeletedUserRow> rows = await cursorExecutor.QueryAsync<DeletedUserRow>(
            DeleteUserCall,
            FetchDeletedUser,
            new { UserId = userId },
            cancellationToken);

        return rows.Count switch
        {
            0 => null,
            1 => rows[0].Id,
            _ => throw new InvalidOperationException("delete_user returned more than one row."),
        };
    }

    private static object ToParameters(UserInput input, long? userId = null)
    {
        return new
        {
            UserId = userId,
            input.Name,
            input.Phone,
            CountryId = (short)input.CountryId,
            DepartmentId = (short)input.DepartmentId,
            input.MunicipalityId,
            input.Address,
        };
    }

    private static UserReference MapRequiredSingle(
        IReadOnlyList<UserRow> rows,
        string procedureName)
    {
        return rows.Count switch
        {
            1 => Map(rows[0]),
            _ => throw new InvalidOperationException(
                $"{procedureName} returned {rows.Count} rows instead of one."),
        };
    }

    private static UserReference Map(UserRow row)
    {
        return new UserReference(
            row.Id,
            row.Name,
            row.Phone,
            row.CountryId,
            row.DepartmentId,
            row.MunicipalityId,
            row.Address,
            row.CreatedAt,
            row.UpdatedAt);
    }

    private static UserReference Map(UserListRow row)
    {
        return new UserReference(
            row.Id!.Value,
            row.Name!,
            row.Phone!,
            row.CountryId!.Value,
            row.DepartmentId!.Value,
            row.MunicipalityId!.Value,
            row.Address!,
            row.CreatedAt!.Value,
            row.UpdatedAt!.Value);
    }

    private sealed record UserRow(
        long Id,
        string Name,
        string Phone,
        short CountryId,
        short DepartmentId,
        int MunicipalityId,
        string Address,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed record UserListRow(
        long? Id,
        string? Name,
        string? Phone,
        short? CountryId,
        short? DepartmentId,
        int? MunicipalityId,
        string? Address,
        DateTime? CreatedAt,
        DateTime? UpdatedAt,
        long TotalCount);

    private sealed record DeletedUserRow(long Id);
}
