namespace BookingService.Application.DTOs;

public record ModifyBookingRequestDto(
    DateTime? NewTravelDate,
    DateTime? NewReturnDate,
    int? NewPassengers,
    string Reason
);

public record BookingModificationResponseDto(
    bool Success,
    string Message,
    decimal ModificationFee,
    decimal NewTotalAmount,
    Guid? ModifiedBookingId
);

public record CancelBookingRequestDto(
    string Reason
);

public record BookingCancellationResponseDto(
    bool Success,
    string Message,
    decimal CancellationFee,
    decimal RefundAmount,
    string RefundStatus
);

