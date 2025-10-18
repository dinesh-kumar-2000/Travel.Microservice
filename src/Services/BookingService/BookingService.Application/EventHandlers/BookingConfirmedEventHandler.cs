using MassTransit;
using BookingService.Contracts.Events;
using SharedKernel.SignalR;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.EventHandlers;

public class BookingConfirmedEventHandler : IConsumer<BookingConfirmedEvent>
{
    private readonly ISignalRNotificationService _signalRService;
    private readonly ILogger<BookingConfirmedEventHandler> _logger;

    public BookingConfirmedEventHandler(
        ISignalRNotificationService signalRService,
        ILogger<BookingConfirmedEventHandler> logger)
    {
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var bookingEvent = context.Message;
        
        _logger.LogInformation("Processing BookingConfirmedEvent for booking {BookingId}", 
            bookingEvent.BookingId);

        // Send real-time notification to the customer
        await _signalRService.NotifyBookingConfirmedAsync("customer-id", new BookingNotification
        {
            BookingId = bookingEvent.BookingId,
            BookingReference = "BK" + bookingEvent.BookingId[..8],
            Status = "Confirmed",
            Message = "Your booking has been confirmed! Payment successful.",
            Timestamp = DateTime.UtcNow
        });
        
        _logger.LogInformation("Sent confirmation notification for booking {BookingId}", 
            bookingEvent.BookingId);
    }
}

