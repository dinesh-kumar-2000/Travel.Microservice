using SharedKernel.Interfaces;

namespace SharedKernel.Models;

public abstract class TenantEntity<TId> : BaseEntity<TId>, ITenantEntity, ISoftDeletable
{
    public string TenantId { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

