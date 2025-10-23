using MediatR;
using Microsoft.Extensions.Logging;
using BookingService.Application.Interfaces;
using BookingService.Application.DTOs;

namespace BookingService.Application.Queries;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDto?>
{
    private readonly IBookingRepository _repository;
    private readonly ILogger<GetBookingByIdQueryHandler> _logger;

    public GetBookingByIdQueryHandler(
        IBookingRepository repository,
        ILogger<GetBookingByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<BookingDto?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting booking {BookingId}", request.BookingId);

        var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            _logger.LogWarning("Booking {BookingId} not found", request.BookingId);
            return null;
        }

        return new BookingDto(
            booking.Id,
            booking.TenantId,
            booking.CustomerId,
            booking.BookingReference,
            booking.PackageId,
            booking.BookingDate,
            booking.TravelDate,
            booking.NumberOfTravelers,
            booking.TotalAmount,
            booking.Currency,
            booking.Status.ToString(),
            booking.PaymentId,
            booking.CreatedAt
        );
    }
}

