using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(Guid id, Guid tenantId);
    Task<List<Flight>> GetAllAsync(Guid tenantId, int page, int limit, string? search, string? status);
    Task<int> GetTotalCountAsync(Guid tenantId, string? search, string? status);
    Task<Flight> CreateAsync(Flight flight);
    Task<Flight?> UpdateAsync(Flight flight);
    Task<bool> DeleteAsync(Guid id, Guid tenantId);
    Task<List<Flight>> SearchFlightsAsync(Guid tenantId, string origin, string destination, DateTime date, int passengers, string? seatClass);
    Task<bool> UpdateSeatAvailabilityAsync(Guid flightId, string seatClass, int count);
}
