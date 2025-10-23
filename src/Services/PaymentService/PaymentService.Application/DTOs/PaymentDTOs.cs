using System.Text.Json.Serialization;

namespace PaymentService.Application.DTOs;

public record CreatePaymentRequest(
    [property: JsonPropertyName("bookingId")] string BookingId,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("paymentMethod")] string PaymentMethod,
    [property: JsonPropertyName("provider")] string Provider, // "stripe" or "razorpay"
    [property: JsonPropertyName("customerEmail")] string CustomerEmail,
    [property: JsonPropertyName("returnUrl")] string? ReturnUrl = null
);

public record CreatePaymentResponse(
    [property: JsonPropertyName("paymentId")] string PaymentId,
    [property: JsonPropertyName("clientSecret")] string? ClientSecret,
    [property: JsonPropertyName("checkoutUrl")] string? CheckoutUrl,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("message")] string Message
);

public record PaymentDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("bookingId")] string BookingId,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("paymentMethod")] string PaymentMethod,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("provider")] string? Provider,
    [property: JsonPropertyName("transactionId")] string? TransactionId,
    [property: JsonPropertyName("providerReference")] string? ProviderReference,
    [property: JsonPropertyName("customerId")] string CustomerId,
    [property: JsonPropertyName("customerEmail")] string? CustomerEmail,
    [property: JsonPropertyName("completedAt")] DateTime? CompletedAt,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);

public record ConfirmPaymentRequest(
    [property: JsonPropertyName("paymentIntentId")] string? PaymentIntentId = null,
    [property: JsonPropertyName("providerReference")] string? ProviderReference = null
);

public record ConfirmPaymentResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("paymentId")] string PaymentId,
    [property: JsonPropertyName("status")] string Status
);

public record RefundPaymentRequest(
    [property: JsonPropertyName("amount")] decimal? Amount,
    [property: JsonPropertyName("reason")] string Reason
);

public record RefundPaymentResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("refundId")] string RefundId,
    [property: JsonPropertyName("refundAmount")] decimal RefundAmount,
    [property: JsonPropertyName("status")] string Status
);

public record PagedPaymentsResponse(
    [property: JsonPropertyName("payments")] IEnumerable<PaymentDto> Payments,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

public record WebhookEvent(
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("eventType")] string EventType,
    [property: JsonPropertyName("paymentId")] string? PaymentId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("rawData")] string RawData
);

