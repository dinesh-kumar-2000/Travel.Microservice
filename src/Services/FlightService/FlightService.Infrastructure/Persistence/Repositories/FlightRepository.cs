using FlightService.Domain.Entities;
using FlightService.Application.Interfaces;
using SharedKernel.Data;
using SharedKernel.Models;

namespace FlightService.Infrastructure.Persistence.Repositories;

public class FlightRepository : BaseRepository<Flight>, IFlightRepository
{
    public FlightRepository(DapperContext context) : base(context)
    {
    }

    public async Task<Flight?> GetByFlightNumberAsync(string flightNumber)
    {
        const string sql = "SELECT * FROM flights WHERE flight_number = @FlightNumber AND is_deleted = false";
        return await QueryFirstOrDefaultAsync<Flight>(sql, new { FlightNumber = flightNumber });
    }

    public async Task<PaginatedResult<Flight>> GetFlightsByRouteAsync(Guid routeId, int page, int pageSize)
    {
        const string sql = @"
            SELECT * FROM flights 
            WHERE route_id = @RouteId AND is_active = true AND is_deleted = false 
            ORDER BY departure_time 
            LIMIT @PageSize OFFSET @Offset";
        
        const string countSql = "SELECT COUNT(*) FROM flights WHERE route_id = @RouteId AND is_active = true AND is_deleted = false";
        
        var offset = (page - 1) * pageSize;
        var items = await QueryAsync<Flight>(sql, new { RouteId = routeId, PageSize = pageSize, Offset = offset });
        var totalCount = await QuerySingleAsync<int>(countSql, new { RouteId = routeId });
        
        return new PaginatedResult<Flight>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime departureDate)
    {
        const string sql = @"
            SELECT f.* FROM flights f
            INNER JOIN flight_routes r ON f.route_id = r.id
            WHERE r.origin_code = @Origin AND r.destination_code = @Destination 
            AND DATE(f.departure_time) = @DepartureDate 
            AND f.is_active = true AND f.is_deleted = false
            ORDER BY f.departure_time";
        
        return await QueryAsync<Flight>(sql, new { 
            Origin = origin, 
            Destination = destination, 
            DepartureDate = departureDate.Date 
        });
    }

    public async Task<IEnumerable<Flight>> GetAvailableFlightsAsync(DateTime departureDate, int passengerCount)
    {
        const string sql = @"
            SELECT * FROM flights 
            WHERE DATE(departure_time) = @DepartureDate 
            AND available_seats >= @PassengerCount 
            AND is_active = true AND is_deleted = false
            ORDER BY departure_time";
        
        return await QueryAsync<Flight>(sql, new { 
            DepartureDate = departureDate.Date, 
            PassengerCount = passengerCount 
        });
    }
}
