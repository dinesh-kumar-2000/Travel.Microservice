using MediatR;
using BookingService.Application.DTOs;

namespace BookingService.Application.Commands;

public record ConfirmBookingCommand(
    string BookingId,
    string PaymentId
) : IRequest<ConfirmBookingResponseDto>;

