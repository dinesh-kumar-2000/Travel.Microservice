namespace BookingService.Contracts.DTOs;

public record LoyaltyPointsDto(
    Guid UserId,
    Guid TenantId,
    int CurrentPoints,
    int LifetimePoints,
    string Tier,
    int PointsExpiring,
    DateTime? ExpiryDate,
    string NextTier,
    int PointsToNextTier
);

public record LoyaltyTransactionDto(
    Guid Id,
    string TransactionType,
    int Points,
    int BalanceAfter,
    string Description,
    string? ReferenceType,
    Guid? ReferenceId,
    DateTime CreatedAt
);

public record LoyaltyRewardDto(
    Guid Id,
    string Title,
    string? Description,
    string Category,
    int PointsCost,
    decimal? ValueAmount,
    string? ImageUrl,
    bool IsAvailable,
    int? StockQuantity
);

public record RedeemPointsRequest(
    Guid RewardId,
    int PointsToRedeem
);

public record RedemptionResponseDto(
    bool Success,
    string Message,
    string? RedemptionCode,
    int RemainingPoints
);

public record RedemptionDto(
    Guid Id,
    Guid RewardId,
    string RewardTitle,
    int PointsSpent,
    string Status,
    string? RedemptionCode,
    DateTime RedeemedAt,
    DateTime? FulfilledAt
);

