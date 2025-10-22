using SharedKernel.Utilities;

namespace EventBus.RabbitMQ;

public class OutboxMessage
{
    public string Id { get; set; } = DefaultProviders.IdGenerator.Generate();
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DefaultProviders.DateTimeProvider.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}

