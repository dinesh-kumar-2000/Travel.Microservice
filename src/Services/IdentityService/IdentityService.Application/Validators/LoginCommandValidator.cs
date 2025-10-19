using FluentValidation;
using IdentityService.Application.Commands;

namespace IdentityService.Application.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");

        // Either Domain OR TenantId must be provided (not both required)
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Domain) || !string.IsNullOrEmpty(x.TenantId))
            .WithMessage("Either Domain or TenantId must be provided");
    }
}

