using ReviewsService.Domain.Entities;
using SharedKernel.Data;

namespace ReviewsService.Domain.Repositories;

public interface IReviewRepository : ITenantBaseRepository<Review, string>
{
    Task<List<Review>> GetByUserIdAsync(string userId, string tenantId);
    Task<List<Review>> GetByServiceAsync(string tenantId, string serviceType, string serviceId, int page, int limit);
    Task<int> GetServiceReviewCountAsync(string serviceId, string serviceType);
    Task<(decimal AverageRating, int TotalReviews)> GetServiceRatingAsync(string serviceId, string serviceType);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(string serviceId, string serviceType);
    Task<List<string>> GetPendingReviewBookingsAsync(string userId, string tenantId);
}

public interface IReviewVoteRepository : ITenantBaseRepository<ReviewVote, string>
{
    Task<ReviewVote?> GetByReviewAndUserAsync(string reviewId, string userId);
}

public interface IReviewResponseRepository : ITenantBaseRepository<ReviewResponse, string>
{
    Task<List<ReviewResponse>> GetByReviewIdAsync(string reviewId);
}

