using FluentValidation;

namespace Coink.Application.Users;

internal sealed class UserListQueryValidator : AbstractValidator<UserListQuery>
{
    public UserListQueryValidator()
    {
        _ = RuleFor(static query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.")
            .OverridePropertyName("page");

        _ = RuleFor(static query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.")
            .OverridePropertyName("pageSize");

        _ = RuleFor(static query => query.Search)
            .MaximumLength(100)
            .WithMessage("Search must not exceed 100 characters.")
            .OverridePropertyName("search");
    }
}
