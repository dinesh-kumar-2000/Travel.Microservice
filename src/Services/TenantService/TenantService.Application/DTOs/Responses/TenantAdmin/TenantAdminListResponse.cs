using TenantService.Application.DTOs.Responses.TenantAdmin;

namespace TenantService.Application.DTOs.Responses.TenantAdmin;

public class TenantAdminListResponse
{
    public List<TenantAdminResponse> Admins { get; set; } = new();
    public int TotalCount { get; set; }
}
