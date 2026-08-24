using Coink.Application.Common;

namespace Coink.Application.Users;

/// <summary>
/// Exposes validated user-management use cases.
/// </summary>
public interface IUserService
{
    /// <summary>Validates and creates one user.</summary>
    /// <param name="input">Untrusted editable fields.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The created user or a validation failure.</returns>
    Task<ApplicationResult<UserReference>> CreateAsync(
        UserInput input,
        CancellationToken cancellationToken);

    /// <summary>Gets one user or a not-found result.</summary>
    /// <param name="userId">Stable user identifier.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The stored user or a not-found failure.</returns>
    Task<ApplicationResult<UserReference>> GetAsync(
        long userId,
        CancellationToken cancellationToken);

    /// <summary>Validates list inputs and returns one page.</summary>
    /// <param name="query">Untrusted pagination and search inputs.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The requested page or a validation failure.</returns>
    Task<ApplicationResult<UserPage>> ListAsync(
        UserListQuery query,
        CancellationToken cancellationToken);

    /// <summary>Validates and replaces one user's editable fields.</summary>
    /// <param name="userId">Stable user identifier.</param>
    /// <param name="input">Untrusted replacement fields.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The updated user, validation failure, or not-found failure.</returns>
    Task<ApplicationResult<UserReference>> UpdateAsync(
        long userId,
        UserInput input,
        CancellationToken cancellationToken);

    /// <summary>Deletes one user or returns a not-found result.</summary>
    /// <param name="userId">Stable user identifier.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The deleted identifier or a not-found failure.</returns>
    Task<ApplicationResult<long>> DeleteAsync(
        long userId,
        CancellationToken cancellationToken);
}
