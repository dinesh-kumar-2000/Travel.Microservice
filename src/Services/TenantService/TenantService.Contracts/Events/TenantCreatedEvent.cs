using EventBus.Events;

namespace TenantService.Contracts.Events;

public class TenantCreatedEvent : IntegrationEvent
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
}

