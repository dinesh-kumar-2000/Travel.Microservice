using Dapper;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Tenancy;

namespace CatalogService.Infrastructure.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITenantContext _tenantContext;

    public FlightRepository(IDbConnectionFactory connectionFactory, ITenantContext tenantContext)
    {
        _connectionFactory = connectionFactory;
        _tenantContext = tenantContext;
    }

    public async Task<Flight?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   flight_number AS FlightNumber,
                   airline AS Airline,
                   departure_airport AS DepartureAirport,
                   arrival_airport AS ArrivalAirport,
                   departure_city AS DepartureCity,
                   arrival_city AS ArrivalCity,
                   departure_country AS DepartureCountry,
                   arrival_country AS ArrivalCountry,
                   departure_time AS DepartureTime,
                   arrival_time AS ArrivalTime,
                   price AS Price,
                   currency AS Currency,
                   total_seats AS TotalSeats,
                   available_seats AS AvailableSeats,
                   flight_class AS FlightClass,
                   status AS Status,
                   aircraft_type AS AircraftType,
                   baggage_allowance_kg AS BaggageAllowanceKg,
                   has_meal AS HasMeal,
                   is_refundable AS IsRefundable,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM flights
            WHERE id = @Id AND tenant_id = @TenantId AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<Flight>(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId 
        });
    }

    public async Task<IEnumerable<Flight>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   flight_number AS FlightNumber,
                   airline AS Airline,
                   departure_airport AS DepartureAirport,
                   arrival_airport AS ArrivalAirport,
                   departure_city AS DepartureCity,
                   arrival_city AS ArrivalCity,
                   departure_country AS DepartureCountry,
                   arrival_country AS ArrivalCountry,
                   departure_time AS DepartureTime,
                   arrival_time AS ArrivalTime,
                   price AS Price,
                   currency AS Currency,
                   total_seats AS TotalSeats,
                   available_seats AS AvailableSeats,
                   flight_class AS FlightClass,
                   status AS Status,
                   aircraft_type AS AircraftType,
                   baggage_allowance_kg AS BaggageAllowanceKg,
                   has_meal AS HasMeal,
                   is_refundable AS IsRefundable,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM flights
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY departure_time DESC";

        return await connection.QueryAsync<Flight>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<string> AddAsync(Flight entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO flights (
                id, tenant_id, flight_number, airline, departure_airport, arrival_airport,
                departure_city, arrival_city, departure_country, arrival_country,
                departure_time, arrival_time, price, currency, total_seats, available_seats,
                flight_class, status, aircraft_type, baggage_allowance_kg, has_meal, is_refundable,
                created_at, is_deleted
            )
            VALUES (
                @Id, @TenantId, @FlightNumber, @Airline, @DepartureAirport, @ArrivalAirport,
                @DepartureCity, @ArrivalCity, @DepartureCountry, @ArrivalCountry,
                @DepartureTime, @ArrivalTime, @Price, @Currency, @TotalSeats, @AvailableSeats,
                @FlightClass, @Status, @AircraftType, @BaggageAllowanceKg, @HasMeal, @IsRefundable,
                @CreatedAt, false
            )";
        
        await connection.ExecuteAsync(sql, entity);
        return entity.Id;
    }

    public async Task UpdateAsync(Flight entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE flights SET
                price = @Price,
                status = @Status,
                available_seats = @AvailableSeats,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE flights 
            SET is_deleted = true,
                deleted_at = @DeletedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            TenantId = _tenantContext.TenantId, 
            DeletedAt = DateTime.UtcNow 
        });
    }

    public async Task<(IEnumerable<Flight> Flights, int TotalCount)> SearchFlightsAsync(
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
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_deleted = false", "status = @Status" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Status", (int)FlightStatus.Scheduled);
        
        if (!string.IsNullOrEmpty(departureCity))
        {
            whereClauses.Add("LOWER(departure_city) = LOWER(@DepartureCity)");
            parameters.Add("DepartureCity", departureCity);
        }
        
        if (!string.IsNullOrEmpty(arrivalCity))
        {
            whereClauses.Add("LOWER(arrival_city) = LOWER(@ArrivalCity)");
            parameters.Add("ArrivalCity", arrivalCity);
        }
        
        if (departureDate.HasValue)
        {
            whereClauses.Add("DATE(departure_time) = @DepartureDate");
            parameters.Add("DepartureDate", departureDate.Value.Date);
        }
        
        if (minPrice.HasValue)
        {
            whereClauses.Add("price >= @MinPrice");
            parameters.Add("MinPrice", minPrice.Value);
        }
        
        if (maxPrice.HasValue)
        {
            whereClauses.Add("price <= @MaxPrice");
            parameters.Add("MaxPrice", maxPrice.Value);
        }
        
        if (!string.IsNullOrEmpty(flightClass) && Enum.TryParse<FlightClass>(flightClass, true, out var flightClassEnum))
        {
            whereClauses.Add("flight_class = @FlightClass");
            parameters.Add("FlightClass", (int)flightClassEnum);
        }
        
        if (!string.IsNullOrEmpty(airline))
        {
            whereClauses.Add("LOWER(airline) LIKE LOWER(@Airline)");
            parameters.Add("Airline", $"%{airline}%");
        }
        
        var whereClause = string.Join(" AND ", whereClauses);
        
        // Get total count
        var countSql = $"SELECT COUNT(*) FROM flights WHERE {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        
        // Get paged data
        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   flight_number AS FlightNumber,
                   airline AS Airline,
                   departure_airport AS DepartureAirport,
                   arrival_airport AS ArrivalAirport,
                   departure_city AS DepartureCity,
                   arrival_city AS ArrivalCity,
                   departure_country AS DepartureCountry,
                   arrival_country AS ArrivalCountry,
                   departure_time AS DepartureTime,
                   arrival_time AS ArrivalTime,
                   price AS Price,
                   currency AS Currency,
                   total_seats AS TotalSeats,
                   available_seats AS AvailableSeats,
                   flight_class AS FlightClass,
                   status AS Status,
                   aircraft_type AS AircraftType,
                   baggage_allowance_kg AS BaggageAllowanceKg,
                   has_meal AS HasMeal,
                   is_refundable AS IsRefundable,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM flights 
            WHERE {whereClause}
            ORDER BY departure_time ASC 
            LIMIT @PageSize OFFSET @Offset";
        
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", offset);
        
        var flights = await connection.QueryAsync<Flight>(dataSql, parameters);
        
        return (flights, totalCount);
    }

    public async Task<IEnumerable<Flight>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   flight_number AS FlightNumber,
                   airline AS Airline,
                   departure_airport AS DepartureAirport,
                   arrival_airport AS ArrivalAirport,
                   departure_city AS DepartureCity,
                   arrival_city AS ArrivalCity,
                   departure_country AS DepartureCountry,
                   arrival_country AS ArrivalCountry,
                   departure_time AS DepartureTime,
                   arrival_time AS ArrivalTime,
                   price AS Price,
                   currency AS Currency,
                   total_seats AS TotalSeats,
                   available_seats AS AvailableSeats,
                   flight_class AS FlightClass,
                   status AS Status,
                   aircraft_type AS AircraftType,
                   baggage_allowance_kg AS BaggageAllowanceKg,
                   has_meal AS HasMeal,
                   is_refundable AS IsRefundable,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM flights
            WHERE tenant_id = @TenantId AND is_deleted = false
            ORDER BY departure_time DESC";

        return await connection.QueryAsync<Flight>(sql, new { TenantId = tenantId });
    }
}

