namespace Coink.Application.Geography;

/// <summary>
/// Represents a country exposed by the immutable geographic reference catalog.
/// </summary>
/// <param name="Id">Stable database identifier.</param>
/// <param name="IsoAlpha2">ISO 3166-1 alpha-2 code.</param>
/// <param name="IsoAlpha3">ISO 3166-1 alpha-3 code.</param>
/// <param name="Name">Display name.</param>
public sealed record CountryReference(
    short Id,
    string IsoAlpha2,
    string IsoAlpha3,
    string Name);

/// <summary>
/// Represents a Colombian department and its country relationship.
/// </summary>
/// <param name="Id">Stable department identifier.</param>
/// <param name="CountryId">Identifier of the owning country.</param>
/// <param name="Code">Two-digit DIVIPOLA department code.</param>
/// <param name="Name">Display name.</param>
public sealed record DepartmentReference(
    short Id,
    short CountryId,
    string Code,
    string Name);

/// <summary>
/// Represents a municipality-level DIVIPOLA unit and its parent relationships.
/// </summary>
/// <param name="Id">Stable municipality identifier.</param>
/// <param name="CountryId">Identifier of the owning country.</param>
/// <param name="DepartmentId">Identifier of the owning department.</param>
/// <param name="Code">Five-digit DIVIPOLA municipality code.</param>
/// <param name="Name">Display name.</param>
public sealed record MunicipalityReference(
    int Id,
    short CountryId,
    short DepartmentId,
    string Code,
    string Name);
