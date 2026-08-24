using Coink.Application.Users;

namespace Coink.Api.Contracts.Users;

internal sealed record UserRequest(
    string? Name,
    string? Phone,
    int CountryId,
    int DepartmentId,
    int MunicipalityId,
    string? Address)
{
    internal UserInput ToApplicationInput()
    {
        return new UserInput(
            Name,
            Phone,
            CountryId,
            DepartmentId,
            MunicipalityId,
            Address);
    }
}

internal sealed record UserResponse(
    long Id,
    string Name,
    string Phone,
    short CountryId,
    short DepartmentId,
    int MunicipalityId,
    string Address,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    internal static UserResponse FromApplication(UserReference user)
    {
        return new UserResponse(
            user.Id,
            user.Name,
            user.Phone,
            user.CountryId,
            user.DepartmentId,
            user.MunicipalityId,
            user.Address,
            user.CreatedAt,
            user.UpdatedAt);
    }
}

internal sealed record UserPageResponse(
    UserResponse[] Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages)
{
    internal static UserPageResponse FromApplication(UserPage page)
    {
        UserResponse[] items =
        [
            .. page.Items.Select(UserResponse.FromApplication),
        ];
        long totalPages = page.TotalCount == 0
            ? 0
            : ((page.TotalCount - 1) / page.PageSize) + 1;

        return new UserPageResponse(
            items,
            page.Page,
            page.PageSize,
            page.TotalCount,
            totalPages);
    }
}
