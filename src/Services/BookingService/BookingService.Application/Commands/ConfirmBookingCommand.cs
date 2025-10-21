using MediatR;
using BookingService.Contracts.DTOs;

namespace BookingService.Application.Commands;

public record ConfirmBookingCommand(
    string BookingId,
    string PaymentId
) : IRequest<ConfirmBookingResponse>;

