namespace BookingService.Contracts.DTOs;

public record CreateBookingRequest(
    string PackageId,
    DateTime TravelDate,
    int NumberOfTravelers,
    string IdempotencyKey
);

public record CreateBookingResponse(
    string BookingId,
    string BookingReference,
    string Status,
    decimal TotalAmount,
    string Message
);

public record BookingDto(
    string Id,
    string BookingReference,
    string PackageId,
    DateTime TravelDate,
    int NumberOfTravelers,
    decimal TotalAmount,
    string Status
);

