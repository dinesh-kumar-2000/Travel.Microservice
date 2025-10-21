using BookingService.Contracts.DTOs;
using MediatR;

namespace BookingService.Application.Commands;

public record CreateLoyaltyAccountCommand(
    Guid UserId,
    Guid TenantId
) : IRequest<LoyaltyPointsDto>;

public record AddLoyaltyTransactionCommand(
    Guid UserId,
    Guid TenantId,
    string TransactionType,
    int Points,
    string Description,
    string? ReferenceType,
    Guid? ReferenceId
) : IRequest<bool>;

public record RedeemLoyaltyPointsCommand(
    Guid UserId,
    Guid TenantId,
    Guid RewardId,
    int PointsToRedeem
) : IRequest<RedemptionResponseDto>;

public record AdjustLoyaltyPointsCommand(
    Guid UserId,
    Guid TenantId,
    int Points,
    string Reason
) : IRequest<bool>;

public record ExpireLoyaltyPointsCommand(
    Guid UserId,
    Guid TenantId
) : IRequest<int>; // Returns number of points expired

