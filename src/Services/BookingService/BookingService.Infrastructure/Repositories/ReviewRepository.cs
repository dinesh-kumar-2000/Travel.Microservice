using Dapper;
using BookingService.Domain.Entities;
using BookingService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Npgsql;
using Tenancy;

namespace BookingService.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ReviewRepository> _logger;

    public ReviewRepository(
        string connectionString,
        ITenantContext tenantContext,
        ILogger<ReviewRepository> logger)
    {
        _connectionString = connectionString;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<Review?> GetByIdAsync(Guid id)
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
                   updated_at AS UpdatedAt
            FROM reviews
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Review>(sql, new { Id = id });
    }

    public async Task<List<Review>> GetByUserIdAsync(Guid userId)
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
                   updated_at AS UpdatedAt
            FROM reviews
            WHERE user_id = @UserId
            ORDER BY created_at DESC";

        var reviews = await connection.QueryAsync<Review>(sql, new { UserId = userId });
        return reviews.ToList();
    }

    public async Task<List<Review>> GetByServiceAsync(
        Guid tenantId,
        string serviceType,
        Guid serviceId,
        int page,
        int limit)
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
                   updated_at AS UpdatedAt
            FROM reviews
            WHERE tenant_id = @TenantId 
              AND service_type = @ServiceType 
              AND service_id = @ServiceId
              AND status = 'published'
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

    public async Task<int> GetServiceReviewCountAsync(Guid serviceId, string serviceType)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT COUNT(*) 
            FROM reviews
            WHERE service_id = @ServiceId 
              AND service_type = @ServiceType
              AND status = 'published'";

        return await connection.ExecuteScalarAsync<int>(sql, new { ServiceId = serviceId, ServiceType = serviceType });
    }

    public async Task<Review> CreateAsync(Review review)
    {
        using var connection = CreateConnection();
        
        review.Id = Guid.NewGuid();
        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO reviews (
                id, user_id, tenant_id, booking_id, service_type, service_id, service_name,
                rating, title, comment, photos, status, helpful_count, not_helpful_count,
                created_at, updated_at
            )
            VALUES (
                @Id, @UserId, @TenantId, @BookingId, @ServiceType, @ServiceId, @ServiceName,
                @Rating, @Title, @Comment, @Photos, @Status, @HelpfulCount, @NotHelpfulCount,
                @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, review);

        _logger.LogInformation("Review created for booking {BookingId}", review.BookingId);
        return review;
    }

    public async Task<Review?> UpdateAsync(Review review)
    {
        using var connection = CreateConnection();
        
        review.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE reviews SET
                rating = @Rating,
                title = @Title,
                comment = @Comment,
                status = @Status,
                moderation_notes = @ModerationNotes,
                moderated_by = @ModeratedBy,
                moderated_at = @ModeratedAt,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, review);
        return rowsAffected > 0 ? review : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = CreateConnection();
        
        const string sql = "DELETE FROM reviews WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    public async Task<ReviewVote?> GetVoteAsync(Guid reviewId, Guid userId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   review_id AS ReviewId,
                   user_id AS UserId,
                   is_helpful AS IsHelpful,
                   created_at AS CreatedAt
            FROM review_votes
            WHERE review_id = @ReviewId AND user_id = @UserId";

        return await connection.QueryFirstOrDefaultAsync<ReviewVote>(sql, 
            new { ReviewId = reviewId, UserId = userId });
    }

    public async Task<ReviewVote> AddVoteAsync(ReviewVote vote)
    {
        using var connection = CreateConnection();
        
        vote.Id = Guid.NewGuid();
        vote.CreatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO review_votes (id, review_id, user_id, is_helpful, created_at)
            VALUES (@Id, @ReviewId, @UserId, @IsHelpful, @CreatedAt)
            ON CONFLICT (review_id, user_id) DO UPDATE
            SET is_helpful = @IsHelpful";

        await connection.ExecuteAsync(sql, vote);
        return vote;
    }

    public async Task<ReviewVote?> UpdateVoteAsync(ReviewVote vote)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            UPDATE review_votes SET
                is_helpful = @IsHelpful
            WHERE review_id = @ReviewId AND user_id = @UserId";

        var rowsAffected = await connection.ExecuteAsync(sql, vote);
        return rowsAffected > 0 ? vote : null;
    }

    public async Task<bool> DeleteVoteAsync(Guid reviewId, Guid userId)
    {
        using var connection = CreateConnection();
        
        const string sql = "DELETE FROM review_votes WHERE review_id = @ReviewId AND user_id = @UserId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { ReviewId = reviewId, UserId = userId });

        return rowsAffected > 0;
    }

    public async Task<ReviewResponse> AddResponseAsync(ReviewResponse response)
    {
        using var connection = CreateConnection();
        
        response.Id = Guid.NewGuid();
        response.CreatedAt = DateTime.UtcNow;
        response.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO review_responses (
                id, review_id, tenant_id, responder_id, responder_name, response,
                created_at, updated_at
            )
            VALUES (
                @Id, @ReviewId, @TenantId, @ResponderId, @ResponderName, @Response,
                @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, response);
        return response;
    }

    public async Task<ReviewResponse?> UpdateResponseAsync(ReviewResponse response)
    {
        using var connection = CreateConnection();
        
        response.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE review_responses SET
                response = @Response,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, response);
        return rowsAffected > 0 ? response : null;
    }

    public async Task<List<ReviewResponse>> GetResponsesByReviewIdAsync(Guid reviewId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   review_id AS ReviewId,
                   tenant_id AS TenantId,
                   responder_id AS ResponderId,
                   responder_name AS ResponderName,
                   response AS Response,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM review_responses
            WHERE review_id = @ReviewId
            ORDER BY created_at ASC";

        var responses = await connection.QueryAsync<ReviewResponse>(sql, new { ReviewId = reviewId });
        return responses.ToList();
    }

    public async Task<(decimal AverageRating, int TotalReviews)> GetServiceRatingAsync(
        Guid serviceId,
        string serviceType)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT AVG(rating)::DECIMAL(3,2) as AverageRating,
                   COUNT(*)::INTEGER as TotalReviews
            FROM reviews
            WHERE service_id = @ServiceId 
              AND service_type = @ServiceType
              AND status = 'published'";

        var result = await connection.QueryFirstOrDefaultAsync<(decimal, int)>(sql, 
            new { ServiceId = serviceId, ServiceType = serviceType });

        return result;
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid serviceId, string serviceType)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT rating, COUNT(*)::INTEGER as count
            FROM reviews
            WHERE service_id = @ServiceId 
              AND service_type = @ServiceType
              AND status = 'published'
            GROUP BY rating
            ORDER BY rating DESC";

        var distribution = await connection.QueryAsync<(int Rating, int Count)>(sql, 
            new { ServiceId = serviceId, ServiceType = serviceType });

        return distribution.ToDictionary(x => x.Rating, x => x.Count);
    }

    public async Task<List<Guid>> GetPendingReviewBookingsAsync(Guid userId, Guid tenantId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT b.id
            FROM bookings b
            LEFT JOIN reviews r ON r.booking_id = b.id
            WHERE b.user_id = @UserId 
              AND b.tenant_id = @TenantId
              AND b.status = 'completed'
              AND r.id IS NULL
              AND (
                (b.return_date IS NOT NULL AND b.return_date < CURRENT_DATE) OR
                (b.return_date IS NULL AND b.travel_date < CURRENT_DATE)
              )";

        var bookingIds = await connection.QueryAsync<Guid>(sql, 
            new { UserId = userId, TenantId = tenantId });

        return bookingIds.ToList();
    }
}

