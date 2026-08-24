using Coink.Application;
using Coink.Application.Geography;
using Microsoft.Extensions.DependencyInjection;

namespace Coink.UnitTests.Geography;

public sealed class GeographyServiceTests
{
    [Fact]
    public async Task ServicePreservesRepositoryMissingParentSemantics()
    {
        var services = new ServiceCollection();
        _ = services.AddSingleton<IGeographyRepository>(new MissingParentRepository());
        _ = services.AddApplication();
        await using ServiceProvider provider = services.BuildServiceProvider();
        IGeographyService service = provider.GetRequiredService<IGeographyService>();

        IReadOnlyList<DepartmentReference>? departments = await service.GetDepartmentsAsync(
            99,
            CancellationToken.None);
        IReadOnlyList<MunicipalityReference>? municipalities =
            await service.GetMunicipalitiesAsync(99, CancellationToken.None);

        Assert.Null(departments);
        Assert.Null(municipalities);
    }

    private sealed class MissingParentRepository : IGeographyRepository
    {
        public Task<IReadOnlyList<CountryReference>> GetCountriesAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<CountryReference>>([]);
        }

        public Task<IReadOnlyList<DepartmentReference>?> GetDepartmentsAsync(
            short countryId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<DepartmentReference>?>(null);
        }

        public Task<IReadOnlyList<MunicipalityReference>?> GetMunicipalitiesAsync(
            short departmentId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<MunicipalityReference>?>(null);
        }
    }
}
