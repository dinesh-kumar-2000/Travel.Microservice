using MediatR;

namespace PaymentService.Application.Commands;

public record ProcessPaymentCommand(
    string BookingId,
    string TenantId,
    decimal Amount,
    string PaymentMethod,
    string IdempotencyKey  // Idempotency for payment operations
) : IRequest<ProcessPaymentResponse>;

public record ProcessPaymentResponse(
    string PaymentId,
    string Status,
    string? TransactionId,
    string Message
);

