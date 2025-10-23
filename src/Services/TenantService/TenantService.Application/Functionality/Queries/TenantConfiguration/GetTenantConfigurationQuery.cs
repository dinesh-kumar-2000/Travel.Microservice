using MediatR;
using TenantService.Application.DTOs.Responses.TenantConfiguration;

namespace TenantService.Application.Queries.TenantConfiguration;

public class GetTenantConfigurationQuery : IRequest<TenantConfigurationResponse?>
{
    public Guid TenantId { get; set; }
}
