using NotificationService.Contracts.DTOs;
using MediatR;
using SharedKernel.Models;

namespace NotificationService.Application.Queries;

public record GetUserTicketsQuery(
    Guid UserId,
    string? Status
) : IRequest<List<SupportTicketDto>>;

public record GetTicketByIdQuery(
    Guid TicketId,
    Guid UserId
) : IRequest<SupportTicketDetailDto?>;

public record GetTicketMessagesQuery(
    Guid TicketId,
    Guid UserId
) : IRequest<List<SupportMessageDto>>;

public record GetCannedResponsesQuery(
    Guid TenantId,
    string? Category
) : IRequest<List<CannedResponseDto>>;

public record GetTenantTicketsQuery(
    Guid TenantId,
    string? Status,
    string? Priority,
    Guid? AssignedTo,
    int Page,
    int Limit
) : IRequest<PagedResult<SupportTicketDto>>;

