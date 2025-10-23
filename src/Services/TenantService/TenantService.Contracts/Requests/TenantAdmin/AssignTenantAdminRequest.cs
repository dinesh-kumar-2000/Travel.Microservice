using System.ComponentModel.DataAnnotations;

namespace TenantService.Contracts.Requests.TenantAdmin;

public class AssignTenantAdminRequest
{
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = new();
}
