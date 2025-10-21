using CatalogService.Domain.Entities;
using SharedKernel.Interfaces;

namespace CatalogService.Domain.Repositories;

public interface IFlightRepository : IRepository<Flight, string>
{
    Task<(IEnumerable<Flight> Flights, int TotalCount)> SearchFlightsAsync(
        string tenantId,
        string? departureCity = null,
        string? arrivalCity = null,
        DateTime? departureDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? flightClass = null,
        string? airline = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
        
    Task<IEnumerable<Flight>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}

