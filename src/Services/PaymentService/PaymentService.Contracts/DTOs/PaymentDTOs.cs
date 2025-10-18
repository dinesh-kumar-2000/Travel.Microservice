namespace PaymentService.Contracts.DTOs;

public record CreatePaymentRequest(
    string BookingId,
    decimal Amount,
    string PaymentMethod
);

public record PaymentDto(
    string Id,
    string BookingId,
    decimal Amount,
    string Currency,
    string Status,
    string? TransactionId
);

public record RefundPaymentRequest(
    string PaymentId,
    decimal? Amount,
    string Reason
);

public record RefundResponse(
    string RefundId,
    decimal RefundAmount,
    string Status
);

