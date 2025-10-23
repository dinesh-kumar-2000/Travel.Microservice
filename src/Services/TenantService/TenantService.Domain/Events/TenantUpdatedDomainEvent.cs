using SharedKernel.Interfaces;
using TenantService.Domain.Enums;

namespace TenantService.Domain.Events;

public class TenantUpdatedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }
    public TenantStatus Status { get; }
    public DateTime OccurredOn { get; }
    public string EventId { get; }

    public TenantUpdatedDomainEvent(Guid tenantId, string name, TenantStatus status)
    {
        TenantId = tenantId;
        Name = name;
        Status = status;
        OccurredOn = DateTime.UtcNow;
        EventId = Guid.NewGuid().ToString();
    }
}
