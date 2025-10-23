namespace TenantService.Contracts.Events;

public class TenantDeletedEvent
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}
