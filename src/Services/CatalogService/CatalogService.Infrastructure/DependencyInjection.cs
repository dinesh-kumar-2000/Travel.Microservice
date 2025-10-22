using Microsoft.Extensions.DependencyInjection;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Repositories;
using SharedKernel.Data;
using System.Reflection;

namespace CatalogService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DapperContext from SharedKernel
        services.AddSingleton<IDapperContext>(sp => new DapperContext(connectionString));

        // Register repositories
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<ITourRepository, TourRepository>();
        
        return services;
    }

    public static void InitializeDatabase(string connectionString)
    {
        // Use centralized DatabaseInitializer from SharedKernel
        DatabaseInitializer.Initialize(connectionString, Assembly.GetExecutingAssembly());
    }
}
