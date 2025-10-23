using MediatR;
using NotificationService.Contracts.Responses.Sms;

namespace NotificationService.Application.Commands.Sms;

public class SendSmsCommand : IRequest<SmsResponse>
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
}
