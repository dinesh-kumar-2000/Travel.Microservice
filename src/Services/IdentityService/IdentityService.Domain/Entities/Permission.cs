using SharedKernel.Models;

namespace IdentityService.Domain.Entities;

public class Permission : BaseEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
}
