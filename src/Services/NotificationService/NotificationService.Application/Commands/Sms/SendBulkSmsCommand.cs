using MediatR;
using NotificationService.Contracts.Responses.Sms;

namespace NotificationService.Application.Commands.Sms;

public class SendBulkSmsCommand : IRequest<BulkSmsResponse>
{
    public List<string> Recipients { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
}
