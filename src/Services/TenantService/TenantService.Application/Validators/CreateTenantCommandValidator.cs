using FluentValidation;
using TenantService.Application.Commands;

namespace TenantService.Application.Validators;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required")
            .Matches("^[a-z0-9-]+$").WithMessage("Subdomain must contain only lowercase letters, numbers, and hyphens")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters")
            .MaximumLength(50).WithMessage("Subdomain must not exceed 50 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.ContactPhone)
            .NotEmpty().WithMessage("Contact phone is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");
    }
}

