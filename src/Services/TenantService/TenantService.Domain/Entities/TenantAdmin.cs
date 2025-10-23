using SharedKernel.Models;

namespace TenantService.Domain.Entities;

public class TenantAdmin : BaseEntity<string>
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public static TenantAdmin Create(Guid tenantId, Guid userId, string role, string assignedBy)
    {
        return new TenantAdmin
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            UserId = userId,
            Role = role,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }
}
