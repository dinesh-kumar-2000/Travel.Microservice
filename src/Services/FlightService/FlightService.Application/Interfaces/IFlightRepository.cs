using FlightService.Domain.Entities;
using SharedKernel.Data;
using SharedKernel.Models;

namespace FlightService.Application.Interfaces;

public interface IFlightRepository : IBaseRepository<Flight, string>
{
    Task<Flight?> GetByFlightNumberAsync(string flightNumber);
    Task<PagedResult<Flight>> GetFlightsByRouteAsync(string routeId, int page, int pageSize);
    Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime departureDate);
    Task<IEnumerable<Flight>> GetAvailableFlightsAsync(DateTime departureDate, int passengerCount);
}
