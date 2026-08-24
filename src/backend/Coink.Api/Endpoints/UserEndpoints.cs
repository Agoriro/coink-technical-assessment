using Coink.Api.Contracts.Users;
using Coink.Api.ErrorHandling;
using Coink.Application.Common;
using Coink.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace Coink.Api.Endpoints;

internal static class UserEndpoints
{
    internal static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder users = endpoints
            .MapGroup("/api/v1/users")
            .WithTags("Users");

        _ = users.MapPost("", CreateUserAsync)
            .WithName("CreateUser")
            .WithSummary("Registers a user.")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        _ = users.MapGet("", ListUsersAsync)
            .WithName("ListUsers")
            .WithSummary("Lists users with bounded pagination and basic search.")
            .Produces<UserPageResponse>(StatusCodes.Status200OK)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        _ = users.MapGet("/{id}", GetUserAsync)
            .WithName("GetUser")
            .WithSummary("Gets one user.")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        _ = users.MapPut("/{id}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithSummary("Replaces one user's editable fields.")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        _ = users.MapDelete("/{id}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Permanently deletes one user.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status429TooManyRequests);

        return endpoints;
    }

    private static async Task<IResult> CreateUserAsync(
        UserRequest request,
        IUserService userService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ApplicationResult<UserReference> result = await userService.CreateAsync(
            request.ToApplicationInput(),
            cancellationToken);
        if (!result.IsSuccess)
        {
            return ApiProblemResults.FromApplicationError(httpContext, result.Error!);
        }

        var response = UserResponse.FromApplication(result.Value!);
        return TypedResults.Created($"/api/v1/users/{response.Id}", response);
    }

    private static async Task<IResult> ListUsersAsync(
        IUserService userService,
        HttpContext httpContext,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        string? search = null)
    {
        ApplicationResult<UserPage> result = await userService.ListAsync(
            new UserListQuery(page, pageSize, search),
            cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(UserPageResponse.FromApplication(result.Value!))
            : ApiProblemResults.FromApplicationError(httpContext, result.Error!);
    }

    private static async Task<IResult> GetUserAsync(
        long id,
        IUserService userService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (id < 1)
        {
            return ApiProblemResults.InvalidPositiveIdentifier(httpContext, nameof(id));
        }

        ApplicationResult<UserReference> result = await userService.GetAsync(
            id,
            cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(UserResponse.FromApplication(result.Value!))
            : ApiProblemResults.FromApplicationError(httpContext, result.Error!);
    }

    private static async Task<IResult> UpdateUserAsync(
        long id,
        UserRequest request,
        IUserService userService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (id < 1)
        {
            return ApiProblemResults.InvalidPositiveIdentifier(httpContext, nameof(id));
        }

        ApplicationResult<UserReference> result = await userService.UpdateAsync(
            id,
            request.ToApplicationInput(),
            cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(UserResponse.FromApplication(result.Value!))
            : ApiProblemResults.FromApplicationError(httpContext, result.Error!);
    }

    private static async Task<IResult> DeleteUserAsync(
        long id,
        IUserService userService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (id < 1)
        {
            return ApiProblemResults.InvalidPositiveIdentifier(httpContext, nameof(id));
        }

        ApplicationResult<long> result = await userService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ApiProblemResults.FromApplicationError(httpContext, result.Error!);
    }
}
