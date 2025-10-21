using CatalogService.Contracts.DTOs;
using MediatR;

namespace CatalogService.Application.Queries;

public record GetFlightsQuery(
    Guid TenantId,
    int Page,
    int Limit,
    string? Search,
    string? Status
) : IRequest<PagedResult<FlightDto>>;

public record GetFlightByIdQuery(
    Guid FlightId,
    Guid TenantId
) : IRequest<FlightDto?>;

public record SearchFlightsQuery(
    Guid TenantId,
    string Origin,
    string Destination,
    DateTime DepartureDate,
    int Passengers,
    string? Class
) : IRequest<List<FlightDto>>;

