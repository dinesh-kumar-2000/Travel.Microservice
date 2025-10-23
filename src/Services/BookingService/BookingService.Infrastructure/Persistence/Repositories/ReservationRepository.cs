using Dapper;
using BookingService.Domain.Entities;
using BookingService.Application.Interfaces;
using SharedKernel.Data;
using Microsoft.Extensions.Logging;
using SharedKernel.Models;

namespace BookingService.Infrastructure.Repositories;

public class ReservationRepository : BaseRepository<Reservation, string>, IReservationRepository
{
    protected override string TableName => "reservations";
    protected override string IdColumnName => "id";

    public ReservationRepository(IDapperContext context, ILogger<ReservationRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Reservation>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty", nameof(bookingId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM reservations 
            WHERE booking_id = @BookingId 
            AND is_deleted = false
            ORDER BY created_at";

        return await connection.QueryAsync<Reservation>(sql, new { BookingId = bookingId });
    }

    public async Task<IEnumerable<Reservation>> GetByServiceIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        if (serviceId == Guid.Empty)
            throw new ArgumentException("Service ID cannot be empty", nameof(serviceId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM reservations 
            WHERE service_id = @ServiceId 
            AND is_deleted = false
            ORDER BY created_at";

        return await connection.QueryAsync<Reservation>(sql, new { ServiceId = serviceId });
    }

    public async Task<IEnumerable<Reservation>> GetByStatusAsync(int status, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM reservations 
            WHERE status = @Status 
            AND is_deleted = false
            ORDER BY created_at";

        return await connection.QueryAsync<Reservation>(sql, new { Status = status });
    }

    public async Task<IEnumerable<Reservation>> GetByServiceTypeAsync(int serviceType, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT * FROM reservations 
            WHERE service_type = @ServiceType 
            AND is_deleted = false
            ORDER BY created_at";

        return await connection.QueryAsync<Reservation>(sql, new { ServiceType = serviceType });
    }

    public async Task<bool> ExistsForServiceAsync(Guid serviceId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        if (serviceId == Guid.Empty)
            throw new ArgumentException("Service ID cannot be empty", nameof(serviceId));

        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(*) FROM reservations 
            WHERE service_id = @ServiceId 
            AND ((start_date <= @EndDate AND end_date >= @StartDate))
            AND is_deleted = false";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { 
            ServiceId = serviceId, 
            StartDate = startDate, 
            EndDate = endDate 
        });

        return count > 0;
    }

    public new async Task<PaginatedResult<Reservation>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        
        const string countSql = "SELECT COUNT(*) FROM reservations WHERE is_deleted = false";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        const string sql = @"
            SELECT * FROM reservations 
            WHERE is_deleted = false
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        var offset = (pageNumber - 1) * pageSize;
        var items = await connection.QueryAsync<Reservation>(sql, new { PageSize = pageSize, Offset = offset });

        return new PaginatedResult<Reservation>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public override async Task<string> AddAsync(Reservation entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            INSERT INTO reservations (id, booking_id, service_type, service_id, quantity, unit_price, total_price, 
                                    start_date, end_date, status, special_requests, created_at, created_by, tenant_id)
            VALUES (@Id, @BookingId, @ServiceType, @ServiceId, @Quantity, @UnitPrice, @TotalPrice, 
                    @StartDate, @EndDate, @Status, @SpecialRequests, @CreatedAt, @CreatedBy, @TenantId)
            RETURNING id";

        var id = await connection.ExecuteScalarAsync<string>(sql, entity);
        return id;
    }

    public override async Task<bool> UpdateAsync(Reservation entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE reservations 
            SET booking_id = @BookingId, service_type = @ServiceType, service_id = @ServiceId, 
                quantity = @Quantity, unit_price = @UnitPrice, total_price = @TotalPrice,
                start_date = @StartDate, end_date = @EndDate, status = @Status, 
                special_requests = @SpecialRequests, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false";

        var rowsAffected = await connection.ExecuteAsync(sql, entity);
        return rowsAffected > 0;
    }
}
