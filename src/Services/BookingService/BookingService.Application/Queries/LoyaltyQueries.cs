using BookingService.Contracts.DTOs;
using MediatR;

namespace BookingService.Application.Queries;

public record GetLoyaltyPointsQuery(
    Guid UserId,
    Guid TenantId
) : IRequest<LoyaltyPointsDto>;

public record GetLoyaltyTransactionsQuery(
    Guid UserId,
    Guid TenantId,
    int Page,
    int Limit
) : IRequest<List<LoyaltyTransactionDto>>;

public record GetLoyaltyRewardsQuery(
    Guid TenantId,
    string? Category
) : IRequest<List<LoyaltyRewardDto>>;

public record GetRedemptionHistoryQuery(
    Guid UserId,
    Guid TenantId
) : IRequest<List<RedemptionDto>>;

public record GetLoyaltyTiersQuery(
    Guid TenantId
) : IRequest<List<LoyaltyTierDto>>;

