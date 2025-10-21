using BookingService.Contracts.DTOs;
using MediatR;

namespace BookingService.Application.Commands;

public record ModifyBookingCommand(
    Guid BookingId,
    Guid UserId,
    DateTime? NewTravelDate,
    DateTime? NewReturnDate,
    int? NewPassengers,
    string Reason
) : IRequest<BookingModificationResponseDto>;

public record CancelBookingCommand(
    Guid BookingId,
    Guid UserId,
    string Reason
) : IRequest<BookingCancellationResponseDto>;

public record AddLoyaltyPointsCommand(
    Guid UserId,
    Guid TenantId,
    int Points,
    string Description,
    string ReferenceType,
    Guid ReferenceId
) : IRequest<bool>;

