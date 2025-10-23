using FlightService.Application.Interfaces;
using FlightService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FlightService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IAirlineRepository, AirlineRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IFlightRouteRepository, FlightRouteRepository>();
        
        return services;
    }
}
