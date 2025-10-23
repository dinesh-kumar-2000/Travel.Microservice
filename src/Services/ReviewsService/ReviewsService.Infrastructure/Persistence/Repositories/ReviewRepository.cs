using Dapper;
using ReviewsService.Domain.Entities;
using ReviewsService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace ReviewsService.Infrastructure.Persistence.Repositories;

public class ReviewRepository : TenantBaseRepository<Review, string>, IReviewRepository
{
    protected override string TableName => "reviews";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public ReviewRepository(IDapperContext context, ILogger<ReviewRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<List<Review>> GetByUserIdAsync(string userId, string tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   booking_id AS BookingId,
                   service_type AS ServiceType,
                   service_id AS ServiceId,
                   service_name AS ServiceName,
                   rating AS Rating,
                   title AS Title,
                   comment AS Comment,
                   photos AS Photos,
                   status AS Status,
                   moderation_notes AS ModerationNotes,
                   moderated_by AS ModeratedBy,
                   moderated_at AS ModeratedAt,
                   helpful_count AS HelpfulCount,
                   not_helpful_count AS NotHelpfulCount,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM reviews
            WHERE user_id = @UserId AND tenant_id = @TenantId AND is_deleted = false
            ORDER BY created_at DESC";

        var reviews = await connection.QueryAsync<Review>(sql, 
            new { UserId = userId, TenantId = tenantId });
        
        return reviews.ToList();
    }

    public async Task<List<Review>> GetByServiceAsync(string tenantId, string serviceType, string serviceId, int page, int limit)
    {
        using var connection = CreateConnection();
        
        var offset = (page - 1) * limit;

        const string sql = @"
            SELECT id AS Id,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   booking_id AS BookingId,
                   service_type AS ServiceType,
                   service_id AS ServiceId,
                   service_name AS ServiceName,
                   rating AS Rating,
                   title AS Title,
                   comment AS Comment,
                   photos AS Photos,
                   status AS Status,
                   moderation_notes AS ModerationNotes,
                   moderated_by AS ModeratedBy,
                   moderated_at AS ModeratedAt,
                   helpful_count AS HelpfulCount,
                   not_helpful_count AS NotHelpfulCount,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM reviews
            WHERE tenant_id = @TenantId 
              AND service_type = @ServiceType 
              AND service_id = @ServiceId 
              AND status = 'published'
              AND is_deleted = false
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset";

        var reviews = await connection.QueryAsync<Review>(sql, new
        {
            TenantId = tenantId,
            ServiceType = serviceType,
            ServiceId = serviceId,
            Limit = limit,
            Offset = offset
        });

        return reviews.ToList();
    }

    public async Task<int> GetServiceReviewCountAsync(string serviceId, string serviceType)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(*) 
            FROM reviews
            WHERE service_id = @ServiceId 
              AND service_type = @ServiceType 
              AND status = 'published'
              AND is_deleted = false";

        return await connection.ExecuteScalarAsync<int>(sql, 
            new { ServiceId = serviceId, ServiceType = serviceType });
    }

    public async Task<(decimal AverageRating, int TotalReviews)> GetServiceRatingAsync(string serviceId, string serviceType)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT AVG(rating) AS AverageRating, 
                   COUNT(*) AS TotalReviews
            FROM reviews
            WHERE service_id = @ServiceId 
              AND service_type = @ServiceType 
              AND status = 'published'
              AND is_deleted = false";

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, 
            new { ServiceId = serviceId, ServiceType = serviceType });

        return (
            result?.AverageRating ?? 0m,
            result?.TotalReviews ?? 0
        );
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(string serviceId, string serviceType)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT rating AS Rating, 
                   COUNT(*) AS Count
            FROM reviews
            WHERE service_id = @ServiceId 
              AND service_type = @ServiceType 
              AND status = 'published'
              AND is_deleted = false
            GROUP BY rating
            ORDER BY rating DESC";

        var distribution = await connection.QueryAsync<(int Rating, int Count)>(sql, 
            new { ServiceId = serviceId, ServiceType = serviceType });

        return distribution.ToDictionary(x => x.Rating, x => x.Count);
    }

    public async Task<List<string>> GetPendingReviewBookingsAsync(string userId, string tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT DISTINCT booking_id AS BookingId
            FROM reviews
            WHERE user_id = @UserId 
              AND tenant_id = @TenantId 
              AND status = 'pending'
              AND is_deleted = false";

        var bookingIds = await connection.QueryAsync<string>(sql, 
            new { UserId = userId, TenantId = tenantId });

        return bookingIds.ToList();
    }
}

