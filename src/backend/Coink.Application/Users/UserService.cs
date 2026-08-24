using Coink.Application.Common;
using FluentValidation;
using FluentValidation.Results;

namespace Coink.Application.Users;

internal sealed class UserService(
    IUserRepository repository,
    IValidator<UserInput> inputValidator,
    IValidator<UserListQuery> listQueryValidator) : IUserService
{
    public async Task<ApplicationResult<UserReference>> CreateAsync(
        UserInput input,
        CancellationToken cancellationToken)
    {
        ApplicationError? validationError = await ValidateAsync(
            inputValidator,
            input,
            cancellationToken);
        if (validationError is not null)
        {
            return ApplicationResults.Failure<UserReference>(validationError);
        }

        UserPersistenceResult result = await repository.CreateAsync(input, cancellationToken);
        return MapWriteResult(result);
    }

    public async Task<ApplicationResult<UserReference>> GetAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        UserReference? user = await repository.GetAsync(userId, cancellationToken);
        return user is null
            ? ApplicationResults.Failure<UserReference>(UserNotFound(userId))
            : ApplicationResults.Success(user);
    }

    public async Task<ApplicationResult<UserPage>> ListAsync(
        UserListQuery query,
        CancellationToken cancellationToken)
    {
        ApplicationError? validationError = await ValidateAsync(
            listQueryValidator,
            query,
            cancellationToken);
        if (validationError is not null)
        {
            return ApplicationResults.Failure<UserPage>(validationError);
        }

        UserPage page = await repository.ListAsync(query, cancellationToken);
        return ApplicationResults.Success(page);
    }

    public async Task<ApplicationResult<UserReference>> UpdateAsync(
        long userId,
        UserInput input,
        CancellationToken cancellationToken)
    {
        ApplicationError? validationError = await ValidateAsync(
            inputValidator,
            input,
            cancellationToken);
        if (validationError is not null)
        {
            return ApplicationResults.Failure<UserReference>(validationError);
        }

        UserPersistenceResult result = await repository.UpdateAsync(
            userId,
            input,
            cancellationToken);
        return MapWriteResult(result, userId);
    }

    public async Task<ApplicationResult<long>> DeleteAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        long? deletedUserId = await repository.DeleteAsync(userId, cancellationToken);
        return deletedUserId is null
            ? ApplicationResults.Failure<long>(UserNotFound(userId))
            : ApplicationResults.Success(deletedUserId.Value);
    }

    private static async Task<ApplicationError?> ValidateAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(
            instance,
            cancellationToken);
        if (validationResult.IsValid)
        {
            return null;
        }

        var errors = validationResult.Errors
            .GroupBy(static failure => failure.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .Select(static failure => failure.ErrorMessage)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        return new ApplicationError(
            ApplicationErrorType.Validation,
            "validation.failed",
            "One or more values are invalid.",
            errors);
    }

    private static ApplicationResult<UserReference> MapWriteResult(
        UserPersistenceResult result,
        long? userId = null)
    {
        return result.Status switch
        {
            UserPersistenceStatus.Success when result.User is not null =>
                ApplicationResults.Success(result.User),
            UserPersistenceStatus.NotFound when userId.HasValue =>
                ApplicationResults.Failure<UserReference>(UserNotFound(userId.Value)),
            UserPersistenceStatus.InvalidGeography =>
                ApplicationResults.Failure<UserReference>(new ApplicationError(
                    ApplicationErrorType.InvalidGeography,
                    "geography.invalid_hierarchy",
                    "Country, department, and municipality must form one valid hierarchy.")),
            _ => throw new InvalidOperationException("User persistence returned an invalid state."),
        };
    }

    private static ApplicationError UserNotFound(long userId)
    {
        return new ApplicationError(
            ApplicationErrorType.NotFound,
            "users.not_found",
            $"User {userId} was not found.");
    }
}
