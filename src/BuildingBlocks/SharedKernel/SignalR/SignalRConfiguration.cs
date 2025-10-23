using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.SignalR;

public static class SignalRConfiguration
{
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        });

        services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRealTimeService, RealTimeService>();
        services.AddScoped<IUserConnectionManager, UserConnectionManager>();

        return services;
    }

    public static IServiceCollection AddSignalRWithRedisBackplane(this IServiceCollection services, string connectionString)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.HandshakeTimeout = TimeSpan.FromSeconds(15);
        })
        .AddStackExchangeRedis(connectionString, options =>
        {
            options.Configuration.ChannelPrefix = "TravelPortal";
        });

        services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRealTimeService, RealTimeService>();
        services.AddScoped<IUserConnectionManager, UserConnectionManager>();

        return services;
    }
}

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.Identity?.Name ?? connection.ConnectionId;
    }
}
