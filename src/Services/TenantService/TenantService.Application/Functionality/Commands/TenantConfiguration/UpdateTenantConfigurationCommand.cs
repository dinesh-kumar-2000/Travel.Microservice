using MediatR;
using TenantService.Application.DTOs.Responses.TenantConfiguration;

namespace TenantService.Application.Commands.TenantConfiguration;

public class UpdateTenantConfigurationCommand : IRequest<TenantConfigurationResponse>
{
    public Guid TenantId { get; set; }
    public Dictionary<string, object>? ThemeSettings { get; set; }
    public Dictionary<string, bool>? FeatureFlags { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
    public Dictionary<string, object>? BrandingSettings { get; set; }
}
