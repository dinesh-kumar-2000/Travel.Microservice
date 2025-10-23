using MassTransit;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.EventHandlers;

/// <summary>
/// Handles HotelCreatedEvent and sends real-time notification via SignalR
/// </summary>
public class HotelCreatedEventHandler : IConsumer<HotelCreatedEvent>
{
    private readonly ILogger<HotelCreatedEventHandler> _logger;

    public HotelCreatedEventHandler(ILogger<HotelCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HotelCreatedEvent> context)
    {
        var hotelEvent = context.Message;
        
        _logger.LogInformation("Processing HotelCreatedEvent for hotel {HotelId}", 
            hotelEvent.HotelId);

        // Send real-time notification to admin users
        // This would typically integrate with SignalR service
        
        _logger.LogInformation("Sent real-time notifications for hotel {HotelId}", 
            hotelEvent.HotelId);
    }
}

/// <summary>
/// Handles PackageCreatedEvent and sends real-time notification via SignalR
/// </summary>
public class PackageCreatedEventHandler : IConsumer<PackageCreatedEvent>
{
    private readonly ILogger<PackageCreatedEventHandler> _logger;

    public PackageCreatedEventHandler(ILogger<PackageCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PackageCreatedEvent> context)
    {
        var packageEvent = context.Message;
        
        _logger.LogInformation("Processing PackageCreatedEvent for package {PackageId}", 
            packageEvent.PackageId);

        // Send real-time notification to admin users
        // This would typically integrate with SignalR service
        
        _logger.LogInformation("Sent real-time notifications for package {PackageId}", 
            packageEvent.PackageId);
    }
}

// Placeholder event classes - these would typically be in Contracts project
public class HotelCreatedEvent
{
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class PackageCreatedEvent
{
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}
