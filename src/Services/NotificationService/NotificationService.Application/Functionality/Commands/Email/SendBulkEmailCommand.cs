using MediatR;
using NotificationService.Application.DTOs.Responses.Email;

namespace NotificationService.Application.Commands.Email;

public class SendBulkEmailCommand : IRequest<BulkEmailResponse>
{
    public List<string> Recipients { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
}
