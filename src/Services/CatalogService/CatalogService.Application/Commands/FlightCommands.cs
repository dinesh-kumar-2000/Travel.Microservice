using CatalogService.Contracts.DTOs;
using MediatR;

namespace CatalogService.Application.Commands;

public record CreateFlightCommand(
    Guid TenantId,
    Guid CreatedBy,
    string FlightNumber,
    string Airline,
    string AircraftType,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string Duration,
    decimal EconomyPrice,
    decimal BusinessPrice,
    decimal FirstClassPrice,
    int EconomySeats,
    int BusinessSeats,
    int FirstClassSeats,
    string? BaggageAllowance,
    bool Meals,
    bool Wifi,
    string? Layovers,
    string? Notes
) : IRequest<FlightDto>;

public record UpdateFlightCommand(
    Guid FlightId,
    Guid TenantId,
    string FlightNumber,
    string Airline,
    string AircraftType,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string Duration,
    decimal EconomyPrice,
    decimal BusinessPrice,
    decimal FirstClassPrice,
    int EconomySeats,
    int BusinessSeats,
    int FirstClassSeats,
    string? BaggageAllowance,
    bool Meals,
    bool Wifi,
    string? Layovers,
    string? Notes,
    string Status
) : IRequest<FlightDto?>;

public record DeleteFlightCommand(
    Guid FlightId,
    Guid TenantId
) : IRequest<bool>;

