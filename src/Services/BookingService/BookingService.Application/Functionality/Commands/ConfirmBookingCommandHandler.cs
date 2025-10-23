using MediatR;
using Microsoft.Extensions.Logging;
using BookingService.Application.Interfaces;
using BookingService.Application.DTOs;
using SharedKernel.Exceptions;

namespace BookingService.Application.Commands;

public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, ConfirmBookingResponseDto>
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<ConfirmBookingCommandHandler> _logger;

    public ConfirmBookingCommandHandler(
        IBookingRepository repository,
        ILogger<ConfirmBookingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConfirmBookingResponseDto> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Confirming booking {BookingId} with payment {PaymentId}", 
            request.BookingId, request.PaymentId);

        var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            _logger.LogWarning("Booking {BookingId} not found", request.BookingId);
            throw new NotFoundException("Booking not found");
        }

        if (booking.Status != Domain.Entities.BookingStatus.Pending)
        {
            _logger.LogWarning("Booking {BookingId} cannot be confirmed. Current status: {Status}", 
                request.BookingId, booking.Status);
            throw new ValidationException("Booking", "Only pending bookings can be confirmed");
        }

        booking.Confirm(request.PaymentId);
        await _repository.UpdateAsync(booking, cancellationToken);

        _logger.LogInformation("Booking {BookingId} confirmed successfully", request.BookingId);

        return new ConfirmBookingResponseDto(
            true,
            "Booking confirmed successfully",
            booking.Id,
            booking.Status.ToString()
        );
    }
}

