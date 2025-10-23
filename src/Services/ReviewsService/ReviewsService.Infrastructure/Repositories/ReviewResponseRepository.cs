using Dapper;
using ReviewsService.Domain.Entities;
using ReviewsService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace ReviewsService.Infrastructure.Repositories;

public class ReviewResponseRepository : TenantBaseRepository<ReviewResponse, string>, IReviewResponseRepository
{
    protected override string TableName => "review_responses";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public ReviewResponseRepository(IDapperContext context, ILogger<ReviewResponseRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<List<ReviewResponse>> GetByReviewIdAsync(string reviewId)
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
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM review_responses
            WHERE review_id = @ReviewId AND is_deleted = false
            ORDER BY created_at ASC";

        var responses = await connection.QueryAsync<ReviewResponse>(sql, 
            new { ReviewId = reviewId });
        
        return responses.ToList();
    }
}

