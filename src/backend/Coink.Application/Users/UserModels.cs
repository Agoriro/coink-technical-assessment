namespace Coink.Application.Users;

/// <summary>
/// Contains editable user fields before persistence.
/// </summary>
/// <param name="Name">User's display name.</param>
/// <param name="Phone">Phone containing 7 to 15 digits and an optional leading plus sign.</param>
/// <param name="CountryId">Selected country identifier.</param>
/// <param name="DepartmentId">Selected department identifier.</param>
/// <param name="MunicipalityId">Selected municipality identifier.</param>
/// <param name="Address">User's street or contact address.</param>
public sealed record UserInput(
    string? Name,
    string? Phone,
    int CountryId,
    int DepartmentId,
    int MunicipalityId,
    string? Address);

/// <summary>
/// Represents one persisted user independent of HTTP and database row models.
/// </summary>
/// <param name="Id">Stable user identifier.</param>
/// <param name="Name">Stored display name.</param>
/// <param name="Phone">Stored phone.</param>
/// <param name="CountryId">Stored country identifier.</param>
/// <param name="DepartmentId">Stored department identifier.</param>
/// <param name="MunicipalityId">Stored municipality identifier.</param>
/// <param name="Address">Stored address.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="UpdatedAt">UTC last-update timestamp.</param>
public sealed record UserReference(
    long Id,
    string Name,
    string Phone,
    short CountryId,
    short DepartmentId,
    int MunicipalityId,
    string Address,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Defines bounded list-user inputs.
/// </summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of rows requested, from 1 through 100.</param>
/// <param name="Search">Optional case-insensitive name or phone fragment.</param>
public sealed record UserListQuery(int Page, int PageSize, string? Search);

/// <summary>
/// Represents one deterministic page and its filtered total.
/// </summary>
/// <param name="Items">Rows in deterministic name and identifier order.</param>
/// <param name="Page">Requested one-based page.</param>
/// <param name="PageSize">Requested bounded page size.</param>
/// <param name="TotalCount">Total rows matching the filter before pagination.</param>
public sealed record UserPage(
    IReadOnlyList<UserReference> Items,
    int Page,
    int PageSize,
    long TotalCount);

/// <summary>
/// Classifies expected outcomes produced by user write persistence.
/// </summary>
public enum UserPersistenceStatus
{
    /// <summary>The write succeeded.</summary>
    Success,

    /// <summary>The target user does not exist.</summary>
    NotFound,

    /// <summary>The geographic identifiers violate the catalog hierarchy.</summary>
    InvalidGeography,
}

/// <summary>
/// Carries a user write outcome without exposing PostgreSQL exceptions.
/// </summary>
/// <param name="Status">Expected persistence outcome.</param>
/// <param name="User">Stored user when <paramref name="Status"/> is successful.</param>
public sealed record UserPersistenceResult(
    UserPersistenceStatus Status,
    UserReference? User = null);
