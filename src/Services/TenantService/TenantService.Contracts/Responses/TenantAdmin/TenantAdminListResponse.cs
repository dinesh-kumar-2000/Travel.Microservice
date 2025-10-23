using TenantService.Contracts.Responses.TenantAdmin;

namespace TenantService.Contracts.Responses.TenantAdmin;

public class TenantAdminListResponse
{
    public List<TenantAdminResponse> Admins { get; set; } = new();
    public int TotalCount { get; set; }
}
