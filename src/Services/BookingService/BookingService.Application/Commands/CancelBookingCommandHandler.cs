using MediatR;
using Microsoft.Extensions.Logging;
using BookingService.Domain.Repositories;
using BookingService.Contracts.DTOs;
using SharedKernel.Exceptions;

namespace BookingService.Application.Commands;

public class CancelBookingCommandHandler : IRequestHandler<UserCancelBookingCommand, CancelBookingResponse>
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IBookingRepository repository,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CancelBookingResponse> Handle(UserCancelBookingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling booking {BookingId} with reason: {Reason}", 
            request.BookingId, request.Reason ?? "No reason provided");

        var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            _logger.LogWarning("Booking {BookingId} not found", request.BookingId);
            throw new NotFoundException("Booking not found");
        }

        try
        {
            booking.Cancel();
            await _repository.UpdateAsync(booking, cancellationToken);

            _logger.LogInformation("Booking {BookingId} cancelled successfully", request.BookingId);

            return new CancelBookingResponse(
                true,
                "Booking cancelled successfully",
                booking.Id,
                booking.Status.ToString()
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to cancel booking {BookingId}", request.BookingId);
            throw new ValidationException("Booking", ex.Message);
        }
    }
}

