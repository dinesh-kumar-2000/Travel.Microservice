using MediatR;
using BookingService.Application.DTOs;

namespace BookingService.Application.Commands;

public record UserCancelBookingCommand(
    string BookingId,
    string? Reason = null
) : IRequest<CancelBookingResponseDto>;

