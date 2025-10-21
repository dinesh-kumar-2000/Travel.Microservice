using BookingService.Contracts.DTOs;
using MediatR;

namespace BookingService.Application.Queries;

public record GetUserReviewsQuery(
    Guid UserId
) : IRequest<List<ReviewDto>>;

public record GetPendingReviewsQuery(
    Guid UserId,
    Guid TenantId
) : IRequest<List<PendingReviewDto>>;

public record GetServiceReviewsQuery(
    Guid TenantId,
    string ServiceType,
    Guid ServiceId,
    int Page,
    int Limit
) : IRequest<ServiceReviewsDto>;

public record GetReviewByIdQuery(
    Guid ReviewId
) : IRequest<ReviewDto?>;

public record GetServiceRatingSummaryQuery(
    Guid ServiceId,
    string ServiceType
) : IRequest<RatingSummaryDto>;

