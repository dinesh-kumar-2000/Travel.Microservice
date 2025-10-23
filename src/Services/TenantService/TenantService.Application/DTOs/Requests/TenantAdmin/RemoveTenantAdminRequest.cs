using System.ComponentModel.DataAnnotations;

namespace TenantService.Application.DTOs.Requests.TenantAdmin;

public class RemoveTenantAdminRequest
{
    [Required]
    public Guid AdminId { get; set; }
}
