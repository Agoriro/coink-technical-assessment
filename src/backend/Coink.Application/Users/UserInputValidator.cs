using System.Text.RegularExpressions;
using FluentValidation;

namespace Coink.Application.Users;

internal sealed partial class UserInputValidator : AbstractValidator<UserInput>
{
    [GeneratedRegex("^\\+?[0-9]{7,15}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();

    public UserInputValidator()
    {
        _ = RuleFor(static input => input.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(150)
            .WithMessage("Name must not exceed 150 characters.")
            .Must(static name => name is null || name == name.Trim())
            .WithMessage("Name must not have leading or trailing whitespace.")
            .OverridePropertyName("name");

        _ = RuleFor(static input => input.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Phone is required.")
            .Must(static phone => phone is null || PhoneRegex().IsMatch(phone))
            .WithMessage("Phone must contain 7 to 15 digits with an optional leading plus sign.")
            .OverridePropertyName("phone");

        _ = RuleFor(static input => input.CountryId)
            .InclusiveBetween(1, short.MaxValue)
            .WithMessage($"Country identifier must be between 1 and {short.MaxValue}.")
            .OverridePropertyName("countryId");

        _ = RuleFor(static input => input.DepartmentId)
            .InclusiveBetween(1, short.MaxValue)
            .WithMessage($"Department identifier must be between 1 and {short.MaxValue}.")
            .OverridePropertyName("departmentId");

        _ = RuleFor(static input => input.MunicipalityId)
            .GreaterThan(0)
            .WithMessage("Municipality identifier must be greater than 0.")
            .OverridePropertyName("municipalityId");

        _ = RuleFor(static input => input.Address)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Address is required.")
            .MaximumLength(250)
            .WithMessage("Address must not exceed 250 characters.")
            .Must(static address => address is null || address == address.Trim())
            .WithMessage("Address must not have leading or trailing whitespace.")
            .OverridePropertyName("address");
    }
}
