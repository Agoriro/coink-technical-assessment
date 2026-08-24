using System.Net;
using System.Net.Http.Json;
using Coink.IntegrationTests.Infrastructure;

namespace Coink.IntegrationTests.Api;

[Collection(IntegrationTestSuite.Name)]
public sealed class UserApiTests(PostgreSqlApiFixture fixture)
{
    [Fact]
    public async Task CrudJourneyReturnsDocumentedStatusesAndPersistsChanges()
    {
        await fixture.ResetUsersAsync();
        UserInputDto input = await ValidInputAsync("Grace Hopper", "+573001111111");

        HttpResponseMessage create = await fixture.Client.PostAsJsonAsync(
            "/api/v1/users",
            input);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        UserDto created = Assert.IsType<UserDto>(await create.Content.ReadFromJsonAsync<UserDto>());
        Assert.Equal($"/api/v1/users/{created.Id}", create.Headers.Location?.OriginalString);

        UserDto? read = await fixture.Client.GetFromJsonAsync<UserDto>(
            $"/api/v1/users/{created.Id}");
        Assert.Equal(input.Name, read?.Name);

        UserInputDto replacement = input with
        {
            Name = "Grace Murray Hopper",
            Address = "Carrera 8 # 9-10",
        };
        HttpResponseMessage update = await fixture.Client.PutAsJsonAsync(
            $"/api/v1/users/{created.Id}",
            replacement);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        UserDto updated = Assert.IsType<UserDto>(await update.Content.ReadFromJsonAsync<UserDto>());
        Assert.Equal(replacement.Address, updated.Address);
        Assert.True(updated.UpdatedAt >= updated.CreatedAt);

        HttpResponseMessage delete = await fixture.Client.DeleteAsync(
            $"/api/v1/users/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await fixture.Client.GetAsync(
            $"/api/v1/users/{created.Id}")).StatusCode);
        Assert.Equal(0, await fixture.CountUsersAsync());
    }

    [Fact]
    public async Task InvalidInputHierarchyAndMissingTargetsReturnSafeFailures()
    {
        await fixture.ResetUsersAsync();
        UserInputDto valid = await ValidInputAsync("Katherine Johnson", "3002222222");

        HttpResponseMessage validation = await fixture.Client.PostAsJsonAsync(
            "/api/v1/users",
            valid with { Phone = "invalid", Name = " Katherine " });
        Assert.Equal(HttpStatusCode.BadRequest, validation.StatusCode);
        ProblemDetailsDto? validationProblem =
            await validation.Content.ReadFromJsonAsync<ProblemDetailsDto>();
        Assert.NotNull(validationProblem?.Errors);
        Assert.Contains("name", validationProblem.Errors.Keys);
        Assert.Contains("phone", validationProblem.Errors.Keys);

        DepartmentDto[] departments = await fixture.Client.GetFromJsonAsync<DepartmentDto[]>(
            $"/api/v1/countries/{valid.CountryId}/departments") ?? [];
        DepartmentDto antioquia = Assert.Single(
            departments,
            department => department.Code == "05");
        DepartmentDto bogota = Assert.Single(
            departments,
            department => department.Code == "11");
        MunicipalityDto[] bogotaMunicipalities =
            await fixture.Client.GetFromJsonAsync<MunicipalityDto[]>(
                $"/api/v1/departments/{bogota.Id}/municipalities") ?? [];
        MunicipalityDto bogotaMunicipality = Assert.Single(bogotaMunicipalities);

        HttpResponseMessage hierarchy = await fixture.Client.PostAsJsonAsync(
            "/api/v1/users",
            valid with
            {
                DepartmentId = antioquia.Id,
                MunicipalityId = bogotaMunicipality.Id,
            });
        Assert.Equal(HttpStatusCode.BadRequest, hierarchy.StatusCode);
        ProblemDetailsDto? hierarchyProblem =
            await hierarchy.Content.ReadFromJsonAsync<ProblemDetailsDto>();
        Assert.Equal("geography.invalid_hierarchy", hierarchyProblem?.Code);

        Assert.Equal(HttpStatusCode.NotFound, (await fixture.Client.GetAsync(
            "/api/v1/users/999999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await fixture.Client.PutAsJsonAsync(
            "/api/v1/users/999999",
            valid)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await fixture.Client.DeleteAsync(
            "/api/v1/users/999999")).StatusCode);
        Assert.Equal(0, await fixture.CountUsersAsync());
    }

    [Fact]
    public async Task ListSearchPaginationAndTieBreakingAreDeterministic()
    {
        await fixture.ResetUsersAsync();
        UserInputDto first = await ValidInputAsync("ana", "3001000001");
        UserInputDto second = first with { Name = "Ana", Phone = "3001000002" };
        UserInputDto third = first with { Name = "Beatriz", Phone = "3001000003" };

        UserDto firstCreated = await CreateAsync(first);
        UserDto secondCreated = await CreateAsync(second);
        _ = await CreateAsync(third);

        UserPageDto? firstPage = await fixture.Client.GetFromJsonAsync<UserPageDto>(
            "/api/v1/users?page=1&pageSize=2");
        Assert.Equal(3, firstPage?.TotalCount);
        Assert.Equal([firstCreated.Id, secondCreated.Id], firstPage?.Items.Select(user => user.Id));

        UserPageDto? secondPage = await fixture.Client.GetFromJsonAsync<UserPageDto>(
            "/api/v1/users?page=2&pageSize=2");
        _ = Assert.Single(secondPage?.Items ?? []);
        Assert.Equal(2, secondPage?.TotalPages);

        UserPageDto? nameSearch = await fixture.Client.GetFromJsonAsync<UserPageDto>(
            "/api/v1/users?page=1&pageSize=10&search=ANA");
        Assert.Equal(2, nameSearch?.TotalCount);

        UserPageDto? phoneSearch = await fixture.Client.GetFromJsonAsync<UserPageDto>(
            "/api/v1/users?page=1&pageSize=10&search=0003");
        Assert.Equal("Beatriz", Assert.Single(phoneSearch?.Items ?? []).Name);

        UserPageDto? emptyPage = await fixture.Client.GetFromJsonAsync<UserPageDto>(
            "/api/v1/users?page=9&pageSize=2");
        Assert.Empty(emptyPage?.Items ?? []);
        Assert.Equal(3, emptyPage?.TotalCount);
    }

    [Fact]
    public async Task DuplicatePhoneIsAcceptedBecauseNoBusinessConflictRuleExists()
    {
        await fixture.ResetUsersAsync();
        UserInputDto input = await ValidInputAsync("First Person", "3009999999");

        HttpResponseMessage first = await fixture.Client.PostAsJsonAsync("/api/v1/users", input);
        HttpResponseMessage second = await fixture.Client.PostAsJsonAsync(
            "/api/v1/users",
            input with { Name = "Second Person" });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.NotEqual(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal(2, await fixture.CountUsersAsync());
    }

    private async Task<UserInputDto> ValidInputAsync(string name, string phone)
    {
        CountryDto country = Assert.Single(
            await fixture.Client.GetFromJsonAsync<CountryDto[]>("/api/v1/countries") ?? []);
        DepartmentDto department = Assert.Single(
            await fixture.Client.GetFromJsonAsync<DepartmentDto[]>(
                $"/api/v1/countries/{country.Id}/departments") ?? [],
            candidate => candidate.Code == "05");
        MunicipalityDto municipality = (
            await fixture.Client.GetFromJsonAsync<MunicipalityDto[]>(
                $"/api/v1/departments/{department.Id}/municipalities") ?? [])[0];

        return new UserInputDto(
            name,
            phone,
            country.Id,
            department.Id,
            municipality.Id,
            "Calle 10 # 20-30");
    }

    private async Task<UserDto> CreateAsync(UserInputDto input)
    {
        HttpResponseMessage response = await fixture.Client.PostAsJsonAsync(
            "/api/v1/users",
            input);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return Assert.IsType<UserDto>(await response.Content.ReadFromJsonAsync<UserDto>());
    }
}
