namespace Coink.Api.Contracts.Geography;

internal sealed record CountryResponse(
    short Id,
    string IsoAlpha2,
    string IsoAlpha3,
    string Name);

internal sealed record DepartmentResponse(
    short Id,
    short CountryId,
    string Code,
    string Name);

internal sealed record MunicipalityResponse(
    int Id,
    short CountryId,
    short DepartmentId,
    string Code,
    string Name);
