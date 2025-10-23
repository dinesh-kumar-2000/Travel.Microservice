using MediatR;
using TenantService.Application.DTOs.Responses.TenantConfiguration;

namespace TenantService.Application.Commands.TenantConfiguration;

public class ResetTenantConfigurationCommand : IRequest<TenantConfigurationResponse>
{
    public Guid TenantId { get; set; }
}
