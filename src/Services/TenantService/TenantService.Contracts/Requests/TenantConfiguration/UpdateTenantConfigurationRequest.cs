using System.ComponentModel.DataAnnotations;

namespace TenantService.Contracts.Requests.TenantConfiguration;

public class UpdateTenantConfigurationRequest
{
    public Dictionary<string, object>? ThemeSettings { get; set; }
    public Dictionary<string, bool>? FeatureFlags { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
    public Dictionary<string, object>? BrandingSettings { get; set; }
}
