using Microsoft.Extensions.DependencyInjection;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Repositories;
using SharedKernel.Data;
using CatalogService.Infrastructure.Data;

namespace CatalogService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DapperContext (IMPROVED: Single source of truth for connections)
        services.AddSingleton<IDapperContext>(sp => new DapperContext(connectionString));

        // Keep IDbConnectionFactory for backward compatibility with existing code
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

        // Register repositories (IMPROVED: Using IDapperContext instead of IDbConnectionFactory)
        services.AddScoped<IPackageRepository, PackageRepository>();
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
