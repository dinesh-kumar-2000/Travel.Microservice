using EventBus.Interfaces;
using EventBus.RabbitMQ;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        string host,
        string username,
        string password,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configure = null)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configure retry policy and dead letter queue
                DeadLetterQueueConfiguration.ConfigureDeadLetterQueue(cfg);

                configure?.Invoke(context, cfg);

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventBus, RabbitMQEventBus>();

        return services;
    }
}


