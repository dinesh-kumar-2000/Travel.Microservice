using BookingService.Contracts.DTOs;
using MediatR;

namespace BookingService.Application.Commands;

public record CreateReviewCommand(
    Guid UserId,
    Guid TenantId,
    Guid BookingId,
    string ServiceType,
    Guid ServiceId,
    string ServiceName,
    int Rating,
    string Title,
    string Comment,
    List<string> Photos
) : IRequest<ReviewDto>;

public record UpdateReviewCommand(
    Guid ReviewId,
    Guid UserId,
    int Rating,
    string Title,
    string Comment
) : IRequest<ReviewDto>;

public record VoteReviewCommand(
    Guid ReviewId,
    Guid UserId,
    bool IsHelpful
) : IRequest<bool>;

public record ModerateReviewCommand(
    Guid ReviewId,
    Guid ModeratorId,
    string NewStatus,
    string? ModerationNotes
) : IRequest<bool>;

public record RespondToReviewCommand(
    Guid ReviewId,
    Guid TenantId,
    Guid ResponderId,
    string ResponderName,
    string Response
) : IRequest<bool>;

