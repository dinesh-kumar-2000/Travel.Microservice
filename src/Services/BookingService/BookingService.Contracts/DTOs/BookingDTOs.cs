using System.Text.Json.Serialization;

namespace BookingService.Contracts.DTOs;

public record CreateBookingRequest(
    [property: JsonPropertyName("packageId")] string PackageId,
    [property: JsonPropertyName("travelDate")] DateTime TravelDate,
    [property: JsonPropertyName("numberOfTravelers")] int NumberOfTravelers,
    [property: JsonPropertyName("idempotencyKey")] string IdempotencyKey
);

public record CreateBookingResponse(
    [property: JsonPropertyName("bookingId")] string BookingId,
    [property: JsonPropertyName("bookingReference")] string BookingReference,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("message")] string Message
);

public record BookingDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("customerId")] string CustomerId,
    [property: JsonPropertyName("bookingReference")] string BookingReference,
    [property: JsonPropertyName("packageId")] string PackageId,
    [property: JsonPropertyName("bookingDate")] DateTime BookingDate,
    [property: JsonPropertyName("travelDate")] DateTime TravelDate,
    [property: JsonPropertyName("numberOfTravelers")] int NumberOfTravelers,
    [property: JsonPropertyName("totalAmount")] decimal TotalAmount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("paymentId")] string? PaymentId,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);

public record ConfirmBookingRequest(
    [property: JsonPropertyName("paymentId")] string PaymentId
);

public record ConfirmBookingResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("bookingId")] string BookingId,
    [property: JsonPropertyName("status")] string Status
);

public record CancelBookingRequest(
    [property: JsonPropertyName("reason")] string? Reason = null
);

public record CancelBookingResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("bookingId")] string BookingId,
    [property: JsonPropertyName("status")] string Status
);

public record PagedBookingsResponse(
    [property: JsonPropertyName("bookings")] IEnumerable<BookingDto> Bookings,
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("totalPages")] int TotalPages
);

