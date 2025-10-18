using EventBus.Interfaces;
using MassTransit;

namespace EventBus.RabbitMQ;

public class RabbitMQEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public RabbitMQEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : class
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}

