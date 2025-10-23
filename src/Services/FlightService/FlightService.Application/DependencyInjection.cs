using FlightService.Application.Commands.Airline.CreateAirlineCommand;
using FlightService.Application.Commands.Airline.UpdateAirlineCommand;
using FlightService.Application.Commands.Airline.DeleteAirlineCommand;
using FlightService.Application.Commands.Flight.CreateFlightCommand;
using FlightService.Application.Commands.Flight.UpdateFlightCommand;
using FlightService.Application.Commands.Flight.DeleteFlightCommand;
using FlightService.Application.Commands.FlightRoute.CreateFlightRouteCommand;
using FlightService.Application.Commands.FlightRoute.UpdateFlightRouteCommand;
using FlightService.Application.Commands.FlightRoute.DeleteFlightRouteCommand;
using FlightService.Application.Queries.Airline.GetAirlineQuery;
using FlightService.Application.Queries.Airline.GetAllAirlinesQuery;
using FlightService.Application.Queries.Flight.GetFlightQuery;
using FlightService.Application.Queries.Flight.GetAllFlightsQuery;
using FlightService.Application.Queries.Flight.SearchFlightsQuery;
using FlightService.Application.Queries.FlightRoute.GetFlightRouteQuery;
using FlightService.Application.Queries.FlightRoute.GetAllFlightRoutesQuery;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FlightService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            // Commands
            cfg.RegisterServicesFromAssemblyContaining<CreateAirlineCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateAirlineCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteAirlineCommand>();
            cfg.RegisterServicesFromAssemblyContaining<CreateFlightCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateFlightCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteFlightCommand>();
            cfg.RegisterServicesFromAssemblyContaining<CreateFlightRouteCommand>();
            cfg.RegisterServicesFromAssemblyContaining<UpdateFlightRouteCommand>();
            cfg.RegisterServicesFromAssemblyContaining<DeleteFlightRouteCommand>();
            
            // Queries
            cfg.RegisterServicesFromAssemblyContaining<GetAirlineQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllAirlinesQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetFlightQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllFlightsQuery>();
            cfg.RegisterServicesFromAssemblyContaining<SearchFlightsQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetFlightRouteQuery>();
            cfg.RegisterServicesFromAssemblyContaining<GetAllFlightRoutesQuery>();
        });

        return services;
    }
}
