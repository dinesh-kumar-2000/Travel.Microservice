namespace NotificationService.Application.DTOs;

public record SupportTicketDto(
    Guid Id,
    string TicketNumber,
    Guid UserId,
    string Subject,
    string Category,
    string Priority,
    string Status,
    Guid? BookingId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastResponseAt
);

public record SupportTicketDetailDto(
    Guid Id,
    string TicketNumber,
    Guid UserId,
    string Subject,
    string Category,
    string Priority,
    string Status,
    Guid? BookingId,
    string Description,
    Guid? AssignedTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastResponseAt,
    int? SatisfactionRating,
    string? SatisfactionFeedback,
    List<SupportMessageDto> Messages
);

public record CreateTicketRequest(
    string Subject,
    string Category,
    string Priority,
    string Description,
    Guid? BookingId
);

public record SupportMessageDto(
    Guid Id,
    Guid TicketId,
    Guid SenderId,
    string SenderType,
    string SenderName,
    string Content,
    List<string>? Attachments,
    bool IsInternalNote,
    DateTime CreatedAt
);

public record AddMessageRequest(
    string Content,
    List<string>? Attachments
);

public record CloseTicketRequest(
    int? SatisfactionRating,
    string? SatisfactionFeedback
);

public record CannedResponseDto(
    Guid Id,
    string Title,
    string Content,
    string? Category,
    string? Shortcut,
    int UsageCount
);

