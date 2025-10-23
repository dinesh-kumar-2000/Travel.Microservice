using SharedKernel.Interfaces;

namespace TenantService.Domain.Events;

public class TenantDeletedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }
    public string Subdomain { get; }
    public DateTime OccurredOn { get; }
    public string EventId { get; }

    public TenantDeletedDomainEvent(Guid tenantId, string name, string subdomain)
    {
        TenantId = tenantId;
        Name = name;
        Subdomain = subdomain;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid().ToString();
    }
}
