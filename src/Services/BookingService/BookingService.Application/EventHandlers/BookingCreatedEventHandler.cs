using MassTransit;
using BookingService.Contracts.Events;
using SharedKernel.SignalR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.EventHandlers;

/// <summary>
/// Handles BookingCreatedEvent and sends real-time notification via SignalR
/// </summary>
public class BookingCreatedEventHandler : IConsumer<BookingCreatedEvent>
{
    private readonly ISignalRNotificationService _signalRService;
    private readonly ILogger<BookingCreatedEventHandler> _logger;

    public BookingCreatedEventHandler(
        ISignalRNotificationService signalRService,
        ILogger<BookingCreatedEventHandler> logger)
    {
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
    {
        var bookingEvent = context.Message;
        
        _logger.LogInformation("Processing BookingCreatedEvent for booking {BookingId}", 
            bookingEvent.BookingId);

        // Send real-time notification to the customer
        await _signalRService.NotifyBookingCreatedAsync(bookingEvent.CustomerId, new BookingNotification
        {
            BookingId = bookingEvent.BookingId,
            BookingReference = "BK" + bookingEvent.BookingId[..8],
            PackageName = "Travel Package",
            Status = "Pending",
            Amount = bookingEvent.Amount,
            Message = "Your booking has been created successfully and is awaiting payment"
        });

        // Broadcast to tenant admins
        await _signalRService.BroadcastToTenantAsync(bookingEvent.TenantId, new GeneralNotification
        {
            Title = "New Booking Created",
            Message = $"New booking created by customer {bookingEvent.CustomerId}",
            Type = "info",
            Data = new Dictionary<string, object>
            {
                ["bookingId"] = bookingEvent.BookingId,
                ["amount"] = bookingEvent.Amount
            }
        });
        
        _logger.LogInformation("Sent real-time notifications for booking {BookingId}", 
            bookingEvent.BookingId);
    }
}

