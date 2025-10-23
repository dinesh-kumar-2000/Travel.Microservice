namespace TenantService.Application.DTOs.Responses.TenantConfiguration;

public class TenantConfigurationResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Dictionary<string, object>? ThemeSettings { get; set; }
    public Dictionary<string, bool>? FeatureFlags { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
    public Dictionary<string, object>? BrandingSettings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}