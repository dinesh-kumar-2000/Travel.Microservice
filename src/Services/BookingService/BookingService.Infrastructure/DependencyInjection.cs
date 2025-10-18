using Microsoft.Extensions.DependencyInjection;
using BookingService.Domain.Repositories;
using BookingService.Infrastructure.Repositories;
using DbUp;
using System.Reflection;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IBookingRepository>(sp => 
        {
            var tenantContext = sp.GetRequiredService<Tenancy.ITenantContext>();
            var cache = sp.GetRequiredService<SharedKernel.Caching.ICacheService>();
            return new BookingRepository(connectionString, tenantContext, cache);
        });
        
        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build();
        
        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
            throw new Exception("Database migration failed", result.Error);
    }
}

