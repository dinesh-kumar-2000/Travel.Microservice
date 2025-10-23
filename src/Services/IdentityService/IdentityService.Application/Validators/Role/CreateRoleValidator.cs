using FluentValidation;
using IdentityService.Application.Commands.Role;

namespace IdentityService.Application.Validators.Role;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters long")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions list cannot be null");
    }
}
