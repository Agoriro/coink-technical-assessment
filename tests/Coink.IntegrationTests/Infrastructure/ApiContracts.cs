namespace Coink.IntegrationTests.Infrastructure;

public sealed record CountryDto(short Id, string IsoAlpha2, string IsoAlpha3, string Name);

public sealed record DepartmentDto(short Id, short CountryId, string Code, string Name);

public sealed record MunicipalityDto(
    int Id,
    short CountryId,
    short DepartmentId,
    string Code,
    string Name);

public sealed record UserInputDto(
    string Name,
    string Phone,
    int CountryId,
    int DepartmentId,
    int MunicipalityId,
    string Address);

public sealed record UserDto(
    long Id,
    string Name,
    string Phone,
    short CountryId,
    short DepartmentId,
    int MunicipalityId,
    string Address,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record UserPageDto(
    UserDto[] Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);

public sealed record ProblemDetailsDto(
    int? Status,
    string? Title,
    string? Detail,
    string? Code,
    string? TraceId,
    Dictionary<string, string[]>? Errors);
