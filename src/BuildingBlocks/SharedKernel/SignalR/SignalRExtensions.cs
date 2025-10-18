using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace SharedKernel.SignalR;

public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRNotifications(
        this IServiceCollection services, 
        string? redisConnection = null)
    {
        var signalRBuilder = services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        });

        // Add Redis backplane for scaling across multiple instances
        if (!string.IsNullOrEmpty(redisConnection))
        {
            signalRBuilder.AddStackExchangeRedis(redisConnection, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("TravelPortal:SignalR:");
            });
        }

        return services;
    }

    public static IApplicationBuilder MapSignalRHubs(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<NotificationHub>("/hubs/notifications");
        });

        return app;
    }
}

