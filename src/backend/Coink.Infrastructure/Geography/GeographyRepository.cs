using Coink.Application.Geography;
using Dapper;
using Npgsql;

namespace Coink.Infrastructure.Geography;

internal sealed class GeographyRepository(NpgsqlDataSource dataSource) : IGeographyRepository
{
    private const string NoDataFoundSqlState = "P0002";
    private const string GetCountriesCall = "call app.get_countries('api_countries');";
    private const string FetchCountries = "fetch all from api_countries;";
    private const string GetDepartmentsCall =
        "call app.get_departments(@CountryId, 'api_departments');";
    private const string FetchDepartments = "fetch all from api_departments;";
    private const string GetMunicipalitiesCall =
        "call app.get_municipalities(@DepartmentId, 'api_municipalities');";
    private const string FetchMunicipalities = "fetch all from api_municipalities;";

    public async Task<IReadOnlyList<CountryReference>> GetCountriesAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CountryRow> rows = await QueryCursorAsync<CountryRow>(
            GetCountriesCall,
            FetchCountries,
            parameters: null,
            cancellationToken);

        return
        [
            .. rows.Select(static row => new CountryReference(
                row.Id,
                row.IsoAlpha2,
                row.IsoAlpha3,
                row.Name)),
        ];
    }

    public async Task<IReadOnlyList<DepartmentReference>?> GetDepartmentsAsync(
        short countryId,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<DepartmentRow> rows = await QueryCursorAsync<DepartmentRow>(
                GetDepartmentsCall,
                FetchDepartments,
                new { CountryId = countryId },
                cancellationToken);

            return
            [
                .. rows.Select(static row => new DepartmentReference(
                    row.Id,
                    row.CountryId,
                    row.Code,
                    row.Name)),
            ];
        }
        catch (PostgresException exception) when (exception.SqlState == NoDataFoundSqlState)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<MunicipalityReference>?> GetMunicipalitiesAsync(
        short departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<MunicipalityRow> rows = await QueryCursorAsync<MunicipalityRow>(
                GetMunicipalitiesCall,
                FetchMunicipalities,
                new { DepartmentId = departmentId },
                cancellationToken);

            return
            [
                .. rows.Select(static row => new MunicipalityReference(
                    row.Id,
                    row.CountryId,
                    row.DepartmentId,
                    row.Code,
                    row.Name)),
            ];
        }
        catch (PostgresException exception) when (exception.SqlState == NoDataFoundSqlState)
        {
            return null;
        }
    }

    private async Task<IReadOnlyList<T>> QueryCursorAsync<T>(
        string callCommandText,
        string fetchCommandText,
        object? parameters,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await dataSource.OpenConnectionAsync(
            cancellationToken);
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync(
            cancellationToken);

        CommandDefinition callCommand = new(
            callCommandText,
            parameters,
            transaction,
            cancellationToken: cancellationToken);
        _ = await connection.ExecuteAsync(callCommand);

        CommandDefinition fetchCommand = new(
            fetchCommandText,
            transaction: transaction,
            cancellationToken: cancellationToken);
        IEnumerable<T> result = await connection.QueryAsync<T>(fetchCommand);
        List<T> rows = [.. result];

        await transaction.CommitAsync(cancellationToken);
        return rows;
    }

    private sealed record CountryRow(
        short Id,
        string IsoAlpha2,
        string IsoAlpha3,
        string Name);

    private sealed record DepartmentRow(
        short Id,
        short CountryId,
        string Code,
        string Name);

    private sealed record MunicipalityRow(
        int Id,
        short CountryId,
        short DepartmentId,
        string Code,
        string Name);
}
