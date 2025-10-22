using SharedKernel.Utilities;

namespace EventBus.Events;

public abstract class IntegrationEvent
{
    protected IntegrationEvent()
        : this(DefaultProviders.DateTimeProvider, DefaultProviders.IdGenerator)
    {
    }

    protected IntegrationEvent(IDateTimeProvider dateTimeProvider, IIdGenerator idGenerator)
    {
        EventId = idGenerator.Generate();
        OccurredOn = dateTimeProvider.UtcNow;
    }

    public string EventId { get; }
    public DateTime OccurredOn { get; }
    public string EventType => GetType().Name;
}

