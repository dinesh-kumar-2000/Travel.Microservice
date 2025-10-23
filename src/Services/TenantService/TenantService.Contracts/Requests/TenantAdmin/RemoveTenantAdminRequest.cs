using System.ComponentModel.DataAnnotations;

namespace TenantService.Contracts.Requests.TenantAdmin;

public class RemoveTenantAdminRequest
{
    [Required]
    public Guid AdminId { get; set; }
}
