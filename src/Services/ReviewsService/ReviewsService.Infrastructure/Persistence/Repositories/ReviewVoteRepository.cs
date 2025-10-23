using Dapper;
using ReviewsService.Domain.Entities;
using ReviewsService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;

namespace ReviewsService.Infrastructure.Persistence.Repositories;

public class ReviewVoteRepository : TenantBaseRepository<ReviewVote, string>, IReviewVoteRepository
{
    protected override string TableName => "review_votes";
    protected override string IdColumnName => "id";
    protected override string TenantIdColumnName => "tenant_id";

    public ReviewVoteRepository(IDapperContext context, ILogger<ReviewVoteRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<ReviewVote?> GetByReviewAndUserAsync(string reviewId, string userId)
    {
        using var connection = CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   review_id AS ReviewId,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   is_helpful AS IsHelpful,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   created_by AS CreatedBy,
                   updated_by AS UpdatedBy,
                   is_deleted AS IsDeleted
            FROM review_votes
            WHERE review_id = @ReviewId AND user_id = @UserId AND is_deleted = false";

        return await connection.QueryFirstOrDefaultAsync<ReviewVote>(sql, 
            new { ReviewId = reviewId, UserId = userId });
    }
}

