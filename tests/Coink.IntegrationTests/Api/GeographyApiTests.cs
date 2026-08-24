using System.Net;
using System.Net.Http.Json;
using Coink.IntegrationTests.Infrastructure;

namespace Coink.IntegrationTests.Api;

[Collection(IntegrationTestSuite.Name)]
public sealed class GeographyApiTests(PostgreSqlApiFixture fixture)
{
    [Fact]
    public async Task CatalogIsCompleteOrderedAndPreservesParentRelationships()
    {
        CountryDto[] countries = await fixture.Client.GetFromJsonAsync<CountryDto[]>(
            "/api/v1/countries") ?? [];

        CountryDto country = Assert.Single(countries);
        Assert.Equal("CO", country.IsoAlpha2);

        DepartmentDto[] departments = await fixture.Client.GetFromJsonAsync<DepartmentDto[]>(
            $"/api/v1/countries/{country.Id}/departments") ?? [];
        Assert.Equal(33, departments.Length);
        DepartmentDto[] repeatedDepartments =
            await fixture.Client.GetFromJsonAsync<DepartmentDto[]>(
                $"/api/v1/countries/{country.Id}/departments") ?? [];
        Assert.Equal(departments, repeatedDepartments);
        Assert.All(departments, department => Assert.Equal(country.Id, department.CountryId));

        var municipalities = new List<MunicipalityDto>();
        foreach (DepartmentDto department in departments)
        {
            MunicipalityDto[] children =
                await fixture.Client.GetFromJsonAsync<MunicipalityDto[]>(
                    $"/api/v1/departments/{department.Id}/municipalities") ?? [];
            Assert.NotEmpty(children);
            Assert.All(
                children,
                municipality =>
                {
                    Assert.Equal(country.Id, municipality.CountryId);
                    Assert.Equal(department.Id, municipality.DepartmentId);
                });
            municipalities.AddRange(children);
        }

        Assert.Equal(1119, municipalities.Count);
        Assert.Equal(1119, municipalities.Select(municipality => municipality.Id).Distinct().Count());

        DepartmentDto firstDepartment = departments[0];
        MunicipalityDto[] repeatedMunicipalities =
            await fixture.Client.GetFromJsonAsync<MunicipalityDto[]>(
                $"/api/v1/departments/{firstDepartment.Id}/municipalities") ?? [];
        Assert.Equal(
            municipalities.Where(municipality => municipality.DepartmentId == firstDepartment.Id),
            repeatedMunicipalities);
    }

    [Fact]
    public async Task MissingAndInvalidParentsReturnStableProblemDetails()
    {
        HttpResponseMessage missing = await fixture.Client.GetAsync(
            "/api/v1/countries/32767/departments");
        HttpResponseMessage invalid = await fixture.Client.GetAsync(
            "/api/v1/departments/0/municipalities");

        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        ProblemDetailsDto? missingProblem = await missing.Content.ReadFromJsonAsync<ProblemDetailsDto>();
        ProblemDetailsDto? invalidProblem = await invalid.Content.ReadFromJsonAsync<ProblemDetailsDto>();
        Assert.Equal("geography.country_not_found", missingProblem?.Code);
        Assert.Equal("validation.failed", invalidProblem?.Code);
        Assert.False(string.IsNullOrWhiteSpace(missingProblem?.TraceId));
        Assert.False(string.IsNullOrWhiteSpace(invalidProblem?.TraceId));
    }
}
