using MassTransit;
using SharedKernel.Constants;

namespace EventBus.RabbitMQ;

public static class DeadLetterQueueConfiguration
{
    /// <summary>
    /// Standard retry intervals for delayed redelivery
    /// </summary>
    private static readonly TimeSpan[] DelayedRetryIntervals = 
    {
        TimeSpan.FromMinutes(5),   // First retry after 5 minutes
        TimeSpan.FromMinutes(15),  // Second retry after 15 minutes
        TimeSpan.FromMinutes(30)   // Third retry after 30 minutes
    };

    /// <summary>
    /// Configure dead letter queue for failed messages with retry and delayed redelivery
    /// </summary>
    public static void ConfigureDeadLetterQueue(this IRabbitMqBusFactoryConfigurator cfg)
    {
        ConfigureRetryPolicy(cfg);
        ConfigureDelayedRedelivery(cfg);
    }

    /// <summary>
    /// Configure immediate retry policy
    /// </summary>
    public static void ConfigureRetryPolicy(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.UseMessageRetry(r =>
        {
            r.Interval(ApplicationConstants.Events.DefaultRetryCount, 
                TimeSpan.FromSeconds(ApplicationConstants.Events.DefaultRetryDelaySeconds));
            r.Handle<Exception>();  // Retry all exceptions
        });
    }

    /// <summary>
    /// Configure delayed redelivery policy
    /// </summary>
    public static void ConfigureDelayedRedelivery(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.UseDelayedRedelivery(r => r.Intervals(DelayedRetryIntervals));
    }

    /// <summary>
    /// Configure specific endpoint with DLQ
    /// </summary>
    public static void ConfigureEndpointWithDLQ(
        this IRabbitMqReceiveEndpointConfigurator endpoint,
        string queueName)
    {
        // Configure message TTL (24 hours)
        endpoint.SetQueueArgument("x-message-ttl", ApplicationConstants.Queue.MessageTtlMilliseconds);

        // Configure dead letter exchange
        endpoint.SetQueueArgument("x-dead-letter-exchange", $"{queueName}_error");
        
        // Enable lazy queue mode for better performance
        endpoint.SetQueueArgument("x-queue-mode", ApplicationConstants.Queue.QueueMode);
    }
}

