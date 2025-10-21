using BookingService.Domain.Entities;

namespace BookingService.Domain.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id);
    Task<List<Review>> GetByUserIdAsync(Guid userId);
    Task<List<Review>> GetByServiceAsync(Guid tenantId, string serviceType, Guid serviceId, int page, int limit);
    Task<int> GetServiceReviewCountAsync(Guid serviceId, string serviceType);
    Task<Review> CreateAsync(Review review);
    Task<Review?> UpdateAsync(Review review);
    Task<bool> DeleteAsync(Guid id);
    
    // Votes
    Task<ReviewVote?> GetVoteAsync(Guid reviewId, Guid userId);
    Task<ReviewVote> AddVoteAsync(ReviewVote vote);
    Task<ReviewVote?> UpdateVoteAsync(ReviewVote vote);
    Task<bool> DeleteVoteAsync(Guid reviewId, Guid userId);
    
    // Responses
    Task<ReviewResponse> AddResponseAsync(ReviewResponse response);
    Task<ReviewResponse?> UpdateResponseAsync(ReviewResponse response);
    Task<List<ReviewResponse>> GetResponsesByReviewIdAsync(Guid reviewId);
    
    // Aggregates
    Task<(decimal AverageRating, int TotalReviews)> GetServiceRatingAsync(Guid serviceId, string serviceType);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid serviceId, string serviceType);
    
    // Pending reviews
    Task<List<Guid>> GetPendingReviewBookingsAsync(Guid userId, Guid tenantId);
}

