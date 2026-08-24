namespace Coink.Application.Users;

/// <summary>
/// Defines Stored Procedure-backed persistence required by user use cases.
/// </summary>
public interface IUserRepository
{
    /// <summary>Creates one user.</summary>
    /// <param name="input">Validated editable fields.</param>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>The created user or an invalid-geography outcome.</returns>
    Task<UserPersistenceResult> CreateAsync(
        UserInput input,
        CancellationToken cancellationToken);

    /// <summary>Gets one user, or <see langword="null"/> when absent.</summary>
    /// <param name="userId">Stable user identifier.</param>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>The stored user, or <see langword="null"/>.</returns>
    Task<UserReference?> GetAsync(long userId, CancellationToken cancellationToken);

    /// <summary>Returns one bounded, searchable, deterministically ordered page.</summary>
    /// <param name="query">Validated pagination and search inputs.</param>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>The requested page with its filtered total.</returns>
    Task<UserPage> ListAsync(UserListQuery query, CancellationToken cancellationToken);

    /// <summary>Replaces editable fields for one user.</summary>
    /// <param name="userId">Stable user identifier.</param>
    /// <param name="input">Validated replacement fields.</param>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>The updated user, not-found outcome, or invalid-geography outcome.</returns>
    Task<UserPersistenceResult> UpdateAsync(
        long userId,
        UserInput input,
        CancellationToken cancellationToken);

    /// <summary>Deletes one user and returns its identifier, or <see langword="null"/> when absent.</summary>
    /// <param name="userId">Stable user identifier.</param>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>The deleted identifier, or <see langword="null"/>.</returns>
    Task<long?> DeleteAsync(long userId, CancellationToken cancellationToken);
}
