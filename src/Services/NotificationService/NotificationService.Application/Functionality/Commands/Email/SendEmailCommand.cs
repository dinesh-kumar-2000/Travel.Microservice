using MediatR;
using NotificationService.Application.DTOs.Responses.Email;

namespace NotificationService.Application.Commands.Email;

public class SendEmailCommand : IRequest<EmailResponse>
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
    public List<string>? Attachments { get; set; }
}
