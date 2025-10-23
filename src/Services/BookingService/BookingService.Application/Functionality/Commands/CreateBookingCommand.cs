using MediatR;
using BookingService.Application.DTOs;
using BookingService.Domain.Entities;
using BookingService.Application.Interfaces;
using BookingService.Contracts.Events;
using SharedKernel.Utilities;
using SharedKernel.Exceptions;
using EventBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Commands;

public record CreateBookingCommand(
    string CustomerId,
    string TenantId,
    string PackageId,
    DateTime TravelDate,
    int NumberOfTravelers,
    decimal TotalAmount,
    string IdempotencyKey
) : IRequest<CreateBookingResponseDto>;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResponseDto>
{
    private readonly IBookingRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public CreateBookingCommandHandler(
        IBookingRepository repository, 
        IEventBus eventBus,
        ILogger<CreateBookingCommandHandler> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<CreateBookingResponseDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // Check idempotency
        var existing = await _repository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing != null)
        {
            _logger.LogInformation("Returning existing booking {BookingId} for idempotency key {IdempotencyKey}", 
                existing.Id, request.IdempotencyKey);
            
            return new CreateBookingResponseDto(
                existing.Id,
                existing.BookingReference,
                existing.Status.ToString(),
                existing.TotalAmount,
                "Booking already exists"
            );
        }

        // Create booking
        var bookingId = UlidGenerator.Generate();
        var booking = Booking.Create(
            bookingId,
            request.TenantId,
            request.CustomerId,
            request.PackageId,
            request.TravelDate,
            request.NumberOfTravelers,
            request.TotalAmount,
            request.IdempotencyKey
        );

        await _repository.AddAsync(booking, cancellationToken);

        _logger.LogInformation("Created booking {BookingId} for customer {CustomerId}", 
            bookingId, request.CustomerId);

        // Publish event
        await _eventBus.PublishAsync(new BookingCreatedEvent
        {
            BookingId = bookingId,
            TenantId = request.TenantId,
            CustomerId = request.CustomerId,
            PackageId = request.PackageId,
            Amount = request.TotalAmount
        }, cancellationToken);

        return new CreateBookingResponseDto(
            bookingId,
            booking.BookingReference,
            booking.Status.ToString(),
            booking.TotalAmount,
            "Booking created successfully"
        );
    }
}

