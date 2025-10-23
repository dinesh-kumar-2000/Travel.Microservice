namespace TenantService.Contracts.Events;

public class TenantUpdatedEvent
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}
