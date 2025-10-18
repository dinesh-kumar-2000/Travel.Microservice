using MassTransit;

namespace EventBus.RabbitMQ;

public static class DeadLetterQueueConfiguration
{
    /// <summary>
    /// Configure dead letter queue for failed messages
    /// </summary>
    public static void ConfigureDeadLetterQueue(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.UseMessageRetry(r =>
        {
            r.Interval(3, TimeSpan.FromSeconds(5));  // Retry 3 times with 5s interval
            r.Handle<Exception>();  // Retry all exceptions
        });

        cfg.UseDelayedRedelivery(r =>
        {
            r.Intervals(
                TimeSpan.FromMinutes(5),   // First retry after 5 minutes
                TimeSpan.FromMinutes(15),  // Second retry after 15 minutes
                TimeSpan.FromMinutes(30)   // Third retry after 30 minutes
            );
        });
    }

    /// <summary>
    /// Configure specific endpoint with DLQ
    /// </summary>
    public static void ConfigureEndpointWithDLQ(
        this IRabbitMqReceiveEndpointConfigurator endpoint,
        string queueName)
    {
        // Configure message TTL (24 hours)
        endpoint.SetQueueArgument("x-message-ttl", 86400000);

        // Configure dead letter exchange
        endpoint.SetQueueArgument("x-dead-letter-exchange", $"{queueName}_error");
        
        // Enable lazy queue mode for better performance
        endpoint.SetQueueArgument("x-queue-mode", "lazy");
    }
}

