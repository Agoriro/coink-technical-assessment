using Coink.Api.Contracts.Geography;
using Coink.Api.ErrorHandling;
using Coink.Application.Geography;
using Microsoft.AspNetCore.Mvc;

namespace Coink.Api.Endpoints;

internal static class GeographyEndpoints
{
    internal static IEndpointRouteBuilder MapGeographyEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder geography = endpoints
            .MapGroup("/api/v1")
            .WithTags("Geography");

        _ = geography.MapGet("/countries", GetCountriesAsync)
            .WithName("GetCountries")
            .WithSummary("Gets the country catalog.")
            .Produces<CountryResponse[]>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        _ = geography.MapGet("/countries/{countryId}/departments", GetDepartmentsAsync)
            .WithName("GetDepartments")
            .WithSummary("Gets departments belonging to a country.")
            .Produces<DepartmentResponse[]>(StatusCodes.Status200OK)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        _ = geography.MapGet(
                "/departments/{departmentId}/municipalities",
                GetMunicipalitiesAsync)
            .WithName("GetMunicipalities")
            .WithSummary("Gets municipality-level units belonging to a department.")
            .Produces<MunicipalityResponse[]>(StatusCodes.Status200OK)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        return endpoints;
    }

    private static async Task<IResult> GetCountriesAsync(
        IGeographyService geographyService,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CountryReference> countries = await geographyService.GetCountriesAsync(
            cancellationToken);

        CountryResponse[] response =
        [
            .. countries.Select(static country => new CountryResponse(
                country.Id,
                country.IsoAlpha2,
                country.IsoAlpha3,
                country.Name)),
        ];

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> GetDepartmentsAsync(
        int countryId,
        IGeographyService geographyService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (countryId is < 1 or > short.MaxValue)
        {
            return ApiProblemResults.InvalidIdentifier(
                httpContext,
                nameof(countryId),
                short.MaxValue);
        }

        IReadOnlyList<DepartmentReference>? departments =
            await geographyService.GetDepartmentsAsync(
                (short)countryId,
                cancellationToken);

        if (departments is null)
        {
            return ApiProblemResults.NotFound(
                httpContext,
                "geography.country_not_found",
                $"Country {countryId} was not found.");
        }

        DepartmentResponse[] response =
        [
            .. departments.Select(static department => new DepartmentResponse(
                department.Id,
                department.CountryId,
                department.Code,
                department.Name)),
        ];

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> GetMunicipalitiesAsync(
        int departmentId,
        IGeographyService geographyService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (departmentId is < 1 or > short.MaxValue)
        {
            return ApiProblemResults.InvalidIdentifier(
                httpContext,
                nameof(departmentId),
                short.MaxValue);
        }

        IReadOnlyList<MunicipalityReference>? municipalities =
            await geographyService.GetMunicipalitiesAsync(
                (short)departmentId,
                cancellationToken);

        if (municipalities is null)
        {
            return ApiProblemResults.NotFound(
                httpContext,
                "geography.department_not_found",
                $"Department {departmentId} was not found.");
        }

        MunicipalityResponse[] response =
        [
            .. municipalities.Select(static municipality => new MunicipalityResponse(
                municipality.Id,
                municipality.CountryId,
                municipality.DepartmentId,
                municipality.Code,
                municipality.Name)),
        ];

        return TypedResults.Ok(response);
    }
}
