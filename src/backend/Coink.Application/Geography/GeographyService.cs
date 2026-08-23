namespace Coink.Application.Geography;

internal sealed class GeographyService(IGeographyRepository repository) : IGeographyService
{
    public Task<IReadOnlyList<CountryReference>> GetCountriesAsync(
        CancellationToken cancellationToken)
    {
        return repository.GetCountriesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<DepartmentReference>?> GetDepartmentsAsync(
        short countryId,
        CancellationToken cancellationToken)
    {
        return repository.GetDepartmentsAsync(countryId, cancellationToken);
    }

    public Task<IReadOnlyList<MunicipalityReference>?> GetMunicipalitiesAsync(
        short departmentId,
        CancellationToken cancellationToken)
    {
        return repository.GetMunicipalitiesAsync(departmentId, cancellationToken);
    }
}
