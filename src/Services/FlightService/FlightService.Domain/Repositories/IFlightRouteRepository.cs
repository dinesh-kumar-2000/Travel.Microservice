using FlightService.Domain.Entities;
using SharedKernel.Data;

namespace FlightService.Domain.Repositories;

public interface IFlightRouteRepository : IBaseRepository<FlightRoute, string>
{
    Task<FlightRoute?> GetByOriginAndDestinationAsync(string origin, string destination);
    Task<IEnumerable<FlightRoute>> GetRoutesByOriginAsync(string origin);
    Task<IEnumerable<FlightRoute>> GetRoutesByDestinationAsync(string destination);
    Task<IEnumerable<FlightRoute>> GetActiveRoutesAsync();
}
