using SharedKernel.Models;

namespace PaymentService.Domain.Entities;

public class PaymentGateway : BaseEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public int GatewayType { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsTestMode { get; set; } = true;
    public Dictionary<string, object>? Configuration { get; set; }
}
