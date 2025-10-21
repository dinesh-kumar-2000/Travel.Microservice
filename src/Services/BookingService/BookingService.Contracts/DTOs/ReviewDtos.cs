using Microsoft.AspNetCore.Http;

namespace BookingService.Contracts.DTOs;

public record ReviewDto(
    Guid Id,
    Guid UserId,
    Guid BookingId,
    string ServiceType,
    Guid ServiceId,
    string ServiceName,
    int Rating,
    string Title,
    string Comment,
    List<string> Photos,
    string Status,
    string? ModerationNotes,
    int HelpfulCount,
    int NotHelpfulCount,
    DateTime CreatedAt
);

public record CreateReviewRequest(
    Guid BookingId,
    string ServiceType,
    Guid ServiceId,
    string ServiceName,
    int Rating,
    string Title,
    string Comment,
    List<IFormFile>? Photos
);

public record UpdateReviewRequest(
    int Rating,
    string Title,
    string Comment
);

public record VoteReviewRequest(
    bool IsHelpful
);

public record PendingReviewDto(
    Guid BookingId,
    string ServiceType,
    Guid ServiceId,
    string ServiceName,
    DateTime TravelDate,
    bool CanReview
);

public record ServiceReviewsDto(
    Guid ServiceId,
    string ServiceType,
    decimal AverageRating,
    int TotalReviews,
    Dictionary<int, int> RatingDistribution,
    List<ReviewDto> Reviews,
    int CurrentPage,
    int TotalPages
);

