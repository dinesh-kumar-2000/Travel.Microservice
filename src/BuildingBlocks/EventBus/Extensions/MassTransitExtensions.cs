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

                // Configure retry policy
                cfg.UseMessageRetry(r =>
                {
                    r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    );
                    r.Handle<Exception>();
                });

                // Configure delayed redelivery
                cfg.UseDelayedRedelivery(r =>
                {
                    r.Intervals(
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromMinutes(15),
                        TimeSpan.FromMinutes(30)
                    );
                });

                // Configure dead letter queue
                DeadLetterQueueConfiguration.ConfigureDeadLetterQueue(cfg);

                configure?.Invoke(context, cfg);

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventBus, RabbitMQEventBus>();

        return services;
    }
}


