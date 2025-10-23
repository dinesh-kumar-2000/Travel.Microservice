using MediatR;
using TenantService.Contracts.Responses.TenantAdmin;

namespace TenantService.Application.Queries.TenantAdmin;

public class GetTenantAdminsQuery : IRequest<IEnumerable<TenantAdminResponse>>
{
    public Guid TenantId { get; set; }
}
