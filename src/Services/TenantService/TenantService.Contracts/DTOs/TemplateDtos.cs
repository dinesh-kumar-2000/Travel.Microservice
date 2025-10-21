namespace TenantService.Contracts.DTOs;

public record TemplateDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string TemplateType,
    string? Category,
    string? Subject,
    string Content,
    List<string> Variables,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record EmailTemplateDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string TemplateType,
    string? Category,
    string Subject,
    string Content,
    List<string> Variables,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record SMSTemplateDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string TemplateType,
    string? Category,
    string Content,
    List<string> Variables,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record TemplatesResponseDto(
    List<EmailTemplateDto> EmailTemplates,
    List<SMSTemplateDto> SMSTemplates
);

public record CreateEmailTemplateRequest(
    string Name,
    string TemplateType,
    string? Category,
    string Subject,
    string Content,
    List<string> Variables,
    bool IsActive
);

public record CreateSMSTemplateRequest(
    string Name,
    string TemplateType,
    string? Category,
    string Content,
    List<string> Variables,
    bool IsActive
);

public record UpdateTemplateRequest(
    string Name,
    string? Subject,
    string Content,
    List<string> Variables,
    bool IsActive
);

