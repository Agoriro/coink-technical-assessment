namespace Coink.Application.Geography;

/// <summary>
/// Defines persistence operations for the read-only geographic reference catalog.
/// </summary>
public interface IGeographyRepository
{
    /// <summary>
    /// Returns all configured countries in deterministic catalog order.
    /// </summary>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>The complete country catalog.</returns>
    Task<IReadOnlyList<CountryReference>> GetCountriesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns departments belonging to a country.
    /// </summary>
    /// <param name="countryId">Identifier of the parent country.</param>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>
    /// The deterministically ordered departments, or <see langword="null"/> when the country
    /// does not exist. An empty list means the country exists but has no departments.
    /// </returns>
    Task<IReadOnlyList<DepartmentReference>?> GetDepartmentsAsync(
        short countryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns municipality-level units belonging to a department.
    /// </summary>
    /// <param name="departmentId">Identifier of the parent department.</param>
    /// <param name="cancellationToken">Cancels the database operation.</param>
    /// <returns>
    /// The deterministically ordered municipalities, or <see langword="null"/> when the
    /// department does not exist. An empty list means the department exists but has no units.
    /// </returns>
    Task<IReadOnlyList<MunicipalityReference>?> GetMunicipalitiesAsync(
        short departmentId,
        CancellationToken cancellationToken);
}
