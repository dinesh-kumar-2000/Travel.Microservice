using FlightService.Domain.Entities;
using FlightService.Application.Interfaces;
using SharedKernel.Data;

namespace FlightService.Infrastructure.Persistence.Repositories;

public class FlightRouteRepository : BaseRepository<FlightRoute>, IFlightRouteRepository
{
    public FlightRouteRepository(DapperContext context) : base(context)
    {
    }

    public async Task<FlightRoute?> GetByOriginAndDestinationAsync(string origin, string destination)
    {
        const string sql = @"
            SELECT * FROM flight_routes 
            WHERE origin_code = @Origin AND destination_code = @Destination 
            AND is_active = true AND is_deleted = false";
        
        return await QueryFirstOrDefaultAsync<FlightRoute>(sql, new { Origin = origin, Destination = destination });
    }

    public async Task<IEnumerable<FlightRoute>> GetRoutesByOriginAsync(string origin)
    {
        const string sql = @"
            SELECT * FROM flight_routes 
            WHERE origin_code = @Origin AND is_active = true AND is_deleted = false 
            ORDER BY destination";
        
        return await QueryAsync<FlightRoute>(sql, new { Origin = origin });
    }

    public async Task<IEnumerable<FlightRoute>> GetRoutesByDestinationAsync(string destination)
    {
        const string sql = @"
            SELECT * FROM flight_routes 
            WHERE destination_code = @Destination AND is_active = true AND is_deleted = false 
            ORDER BY origin";
        
        return await QueryAsync<FlightRoute>(sql, new { Destination = destination });
    }

    public async Task<IEnumerable<FlightRoute>> GetActiveRoutesAsync()
    {
        const string sql = @"
            SELECT * FROM flight_routes 
            WHERE is_active = true AND is_deleted = false 
            ORDER BY origin, destination";
        
        return await QueryAsync<FlightRoute>(sql);
    }
}
