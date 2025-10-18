using FluentValidation;
using BookingService.Application.Commands;

namespace BookingService.Application.Validators;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.PackageId)
            .NotEmpty().WithMessage("Package ID is required");

        RuleFor(x => x.TravelDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Travel date must be in the future");

        RuleFor(x => x.NumberOfTravelers)
            .GreaterThan(0).WithMessage("Number of travelers must be greater than 0")
            .LessThanOrEqualTo(50).WithMessage("Number of travelers cannot exceed 50");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency key is required");
    }
}

