namespace TenantService.Contracts.Events;

public class TenantCreatedEvent
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public int TenantType { get; set; }
    public DateTime CreatedAt { get; set; }
}