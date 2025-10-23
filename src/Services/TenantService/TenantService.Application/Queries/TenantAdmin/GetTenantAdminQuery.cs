using MediatR;
using TenantService.Contracts.Responses.TenantAdmin;

namespace TenantService.Application.Queries.TenantAdmin;

public class GetTenantAdminQuery : IRequest<TenantAdminResponse?>
{
    public Guid AdminId { get; set; }
}
