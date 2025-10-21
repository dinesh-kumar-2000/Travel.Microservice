using NotificationService.Contracts.DTOs;
using MediatR;

namespace NotificationService.Application.Commands;

public record CreateSupportTicketCommand(
    Guid UserId,
    Guid TenantId,
    string Subject,
    string Category,
    string Priority,
    string Description,
    Guid? BookingId
) : IRequest<SupportTicketDto>;

public record AddTicketMessageCommand(
    Guid TicketId,
    Guid SenderId,
    string SenderName,
    string SenderType,
    string Content,
    List<string>? Attachments
) : IRequest<SupportMessageDto>;

public record UpdateTicketStatusCommand(
    Guid TicketId,
    string NewStatus,
    Guid UpdatedBy
) : IRequest<bool>;

public record AssignTicketCommand(
    Guid TicketId,
    Guid AgentId
) : IRequest<bool>;

public record CloseTicketCommand(
    Guid TicketId,
    Guid UserId,
    int? SatisfactionRating,
    string? SatisfactionFeedback
) : IRequest<bool>;

public record CreateCannedResponseCommand(
    Guid TenantId,
    string Title,
    string Content,
    string? Category,
    string? Shortcut
) : IRequest<CannedResponseDto>;

