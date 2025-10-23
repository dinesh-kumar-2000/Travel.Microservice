using MediatR;
using TenantService.Contracts.Responses.TenantConfiguration;

namespace TenantService.Application.Commands.TenantConfiguration;

public class ResetTenantConfigurationCommand : IRequest<TenantConfigurationResponse>
{
    public Guid TenantId { get; set; }
}
