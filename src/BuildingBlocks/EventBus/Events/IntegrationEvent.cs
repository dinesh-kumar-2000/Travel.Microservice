namespace EventBus.Events;

public abstract class IntegrationEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

