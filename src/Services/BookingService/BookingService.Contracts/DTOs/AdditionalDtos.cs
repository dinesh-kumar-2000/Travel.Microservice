namespace BookingService.Contracts.DTOs;

public record LoyaltyTierDto(
    Guid Id,
    string TierName,
    int MinPoints,
    decimal DiscountPercentage,
    Dictionary<string, object>? Benefits
);

public record RatingSummaryDto(
    Guid ServiceId,
    string ServiceType,
    decimal AverageRating,
    int TotalReviews,
    Dictionary<int, int> RatingDistribution
);

public record PagedResult<T>(
    List<T> Data,
    int CurrentPage,
    int TotalPages,
    int TotalCount
);

