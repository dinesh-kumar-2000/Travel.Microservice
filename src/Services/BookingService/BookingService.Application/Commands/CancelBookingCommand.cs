using MediatR;
using BookingService.Contracts.DTOs;

namespace BookingService.Application.Commands;

public record UserCancelBookingCommand(
    string BookingId,
    string? Reason = null
) : IRequest<CancelBookingResponse>;

