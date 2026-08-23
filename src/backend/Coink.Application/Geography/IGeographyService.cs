namespace Coink.Application.Geography;

/// <summary>
/// Exposes application use cases for navigating the geographic reference hierarchy.
/// </summary>
public interface IGeographyService
{
    /// <summary>
    /// Gets the country catalog.
    /// </summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The countries in deterministic display order.</returns>
    Task<IReadOnlyList<CountryReference>> GetCountriesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets departments for an existing country.
    /// </summary>
    /// <param name="countryId">Identifier of the parent country.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>
    /// The departments, or <see langword="null"/> when the country does not exist.
    /// </returns>
    Task<IReadOnlyList<DepartmentReference>?> GetDepartmentsAsync(
        short countryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets municipality-level units for an existing department.
    /// </summary>
    /// <param name="departmentId">Identifier of the parent department.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>
    /// The municipalities, or <see langword="null"/> when the department does not exist.
    /// </returns>
    Task<IReadOnlyList<MunicipalityReference>?> GetMunicipalitiesAsync(
        short departmentId,
        CancellationToken cancellationToken);
}
