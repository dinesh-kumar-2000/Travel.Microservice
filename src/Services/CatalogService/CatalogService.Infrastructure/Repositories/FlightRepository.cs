using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using SharedKernel.Data;
using SharedKernel.Utilities;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Repositories;

/// <summary>
/// Repository for flight operations
/// Inherits common CRUD operations from TenantBaseRepository
/// </summary>
public class FlightRepository : TenantBaseRepository<Flight, Guid>, IFlightRepository
{
    protected override string TableName => "flights";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public FlightRepository(IDapperContext context, ILogger<FlightRepository> logger) 
        : base(context, logger)
    {
    }

    #region Overridden Methods

    public override async Task<Guid> AddAsync(Flight entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        entity.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        entity.EconomyAvailable = entity.EconomySeats;
        entity.BusinessAvailable = entity.BusinessSeats;
        entity.FirstClassAvailable = entity.FirstClassSeats;

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO flights (
                id, tenant_id, flight_number, airline, aircraft_type,
                origin, destination, departure_time, arrival_time, duration,
                status, economy_price, business_price, first_class_price,
                economy_seats, business_seats, first_class_seats,
                economy_available, business_available, first_class_available,
                baggage_allowance, meals, wifi, layovers, notes,
                created_at, updated_at, created_by
            )
            VALUES (
                @Id, @TenantId, @FlightNumber, @Airline, @AircraftType,
                @Origin, @Destination, @DepartureTime, @ArrivalTime, @Duration,
                @Status, @EconomyPrice, @BusinessPrice, @FirstClassPrice,
                @EconomySeats, @BusinessSeats, @FirstClassSeats,
                @EconomyAvailable, @BusinessAvailable, @FirstClassAvailable,
                @BaggageAllowance, @Meals, @Wifi, @Layovers, @Notes,
                @CreatedAt, @UpdatedAt, @CreatedBy
            )";

        await connection.ExecuteAsync(sql, entity);

        _logger.LogInformation("Flight {FlightNumber} created with ID {FlightId}", 
            entity.FlightNumber, entity.Id);

        return entity.Id;
    }

    public override async Task<bool> UpdateAsync(Flight entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE flights SET
                flight_number = @FlightNumber,
                airline = @Airline,
                aircraft_type = @AircraftType,
                origin = @Origin,
                destination = @Destination,
                departure_time = @DepartureTime,
                arrival_time = @ArrivalTime,
                duration = @Duration,
                status = @Status,
                economy_price = @EconomyPrice,
                business_price = @BusinessPrice,
                first_class_price = @FirstClassPrice,
                economy_seats = @EconomySeats,
                business_seats = @BusinessSeats,
                first_class_seats = @FirstClassSeats,
                economy_available = @EconomyAvailable,
                business_available = @BusinessAvailable,
                first_class_available = @FirstClassAvailable,
                baggage_allowance = @BaggageAllowance,
                meals = @Meals,
                wifi = @Wifi,
                layovers = @Layovers,
                notes = @Notes,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Flight {FlightId} updated", entity.Id);
        }

        return rowsAffected > 0;
    }

    #endregion

    #region Domain-Specific Methods

    public async Task<Flight?> GetByIdAsync(Guid id, Guid tenantId)
    {
        // Use inherited method
        return await GetByIdAsync(id, tenantId, default);
    }

    public async Task<List<Flight>> GetAllAsync(
        Guid tenantId,
        int page,
        int limit,
        string? search,
        string? status)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(search))
        {
            whereClauses.Add("(flight_number ILIKE @Search OR airline ILIKE @Search OR origin ILIKE @Search OR destination ILIKE @Search)");
            parameters.Add("Search", $"%{search}%");
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", status);
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var offset = (page - 1) * limit;

        var sql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   flight_number AS FlightNumber,
                   airline AS Airline,
                   aircraft_type AS AircraftType,
                   origin AS Origin,
                   destination AS Destination,
                   departure_time AS DepartureTime,
                   arrival_time AS ArrivalTime,
                   duration AS Duration,
                   status AS Status,
                   economy_price AS EconomyPrice,
                   business_price AS BusinessPrice,
                   first_class_price AS FirstClassPrice,
                   economy_seats AS EconomySeats,
                   business_seats AS BusinessSeats,
                   first_class_seats AS FirstClassSeats,
                   economy_available AS EconomyAvailable,
                   business_available AS BusinessAvailable,
                   first_class_available AS FirstClassAvailable,
                   baggage_allowance AS BaggageAllowance,
                   meals AS Meals,
                   wifi AS Wifi,
                   layovers AS Layovers,
                   notes AS Notes,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy
            FROM flights
            WHERE {whereClause}
            ORDER BY departure_time DESC
            LIMIT @Limit OFFSET @Offset";

        parameters.Add("Limit", limit);
        parameters.Add("Offset", offset);

        var flights = await connection.QueryAsync<Flight>(sql, parameters);
        return flights.ToList();
    }

    public async Task<Flight> CreateAsync(Flight flight)
    {
        await AddAsync(flight, default);
        return flight;
    }

    public async Task<Flight?> UpdateAsync(Flight flight)
    {
        var success = await UpdateAsync(flight, default);
        return success ? flight : null;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid tenantId)
    {
        // Use inherited method
        return await DeleteAsync(id, default);
    }

    public async Task<int> GetTotalCountAsync(Guid tenantId, string? search, string? status)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(search))
        {
            whereClauses.Add("(flight_number ILIKE @Search OR airline ILIKE @Search OR origin ILIKE @Search OR destination ILIKE @Search)");
            parameters.Add("Search", $"%{search}%");
        }

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", status);
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var sql = $"SELECT COUNT(*) FROM flights WHERE {whereClause}";

        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<List<Flight>> SearchFlightsAsync(
        Guid tenantId,
        string origin,
        string destination,
        DateTime date,
        int passengers,
        string? seatClass)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string>
        {
            "tenant_id = @TenantId",
            "origin = @Origin",
            "destination = @Destination",
            "DATE(departure_time) = DATE(@Date)",
            "status = 'active'"
        };

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Origin", origin);
        parameters.Add("Destination", destination);
        parameters.Add("Date", date);

        if (!string.IsNullOrEmpty(seatClass))
        {
            switch (seatClass.ToLower())
            {
                case "economy":
                    whereClauses.Add("economy_available >= @Passengers");
                    break;
                case "business":
                    whereClauses.Add("business_available >= @Passengers");
                    break;
                case "first_class":
                    whereClauses.Add("first_class_available >= @Passengers");
                    break;
            }
            parameters.Add("Passengers", passengers);
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   flight_number AS FlightNumber,
                   airline AS Airline,
                   aircraft_type AS AircraftType,
                   origin AS Origin,
                   destination AS Destination,
                   departure_time AS DepartureTime,
                   arrival_time AS ArrivalTime,
                   duration AS Duration,
                   status AS Status,
                   economy_price AS EconomyPrice,
                   business_price AS BusinessPrice,
                   first_class_price AS FirstClassPrice,
                   economy_seats AS EconomySeats,
                   business_seats AS BusinessSeats,
                   first_class_seats AS FirstClassSeats,
                   economy_available AS EconomyAvailable,
                   business_available AS BusinessAvailable,
                   first_class_available AS FirstClassAvailable,
                   baggage_allowance AS BaggageAllowance,
                   meals AS Meals,
                   wifi AS Wifi,
                   layovers AS Layovers,
                   notes AS Notes,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy
            FROM flights
            WHERE {whereClause}
            ORDER BY departure_time ASC";

        var flights = await connection.QueryAsync<Flight>(sql, parameters);
        return flights.ToList();
    }

    public async Task<bool> UpdateSeatAvailabilityAsync(Guid flightId, string seatClass, int count)
    {
        using var connection = _context.CreateConnection();
        
        string columnToUpdate = seatClass.ToLower() switch
        {
            "economy" => "economy_available",
            "business" => "business_available",
            "first_class" => "first_class_available",
            _ => throw new ArgumentException("Invalid seat class", nameof(seatClass))
        };

        var sql = $@"
            UPDATE flights 
            SET {columnToUpdate} = GREATEST(0, {columnToUpdate} - @Count),
                updated_at = @UpdatedAt
            WHERE id = @FlightId";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            FlightId = flightId,
            Count = count,
            UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow
        });

        return rowsAffected > 0;
    }

    #endregion
}
