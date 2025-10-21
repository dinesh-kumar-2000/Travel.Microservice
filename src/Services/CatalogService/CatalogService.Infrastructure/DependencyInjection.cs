using Microsoft.Extensions.DependencyInjection;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Data;

namespace CatalogService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DbConnectionFactory
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

        // Register repositories
        services.AddScoped<IPackageRepository>(sp => 
        {
            var tenantContext = sp.GetRequiredService<Tenancy.ITenantContext>();
            var cache = sp.GetRequiredService<SharedKernel.Caching.ICacheService>();
            return new PackageRepository(connectionString, tenantContext, cache);
        });

        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<ITourRepository, TourRepository>();
        
        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        DatabaseInitializer.Initialize(connectionString);
    }
}

