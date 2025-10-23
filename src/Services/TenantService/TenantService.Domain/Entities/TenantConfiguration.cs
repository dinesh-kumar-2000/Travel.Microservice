using SharedKernel.Models;

namespace TenantService.Domain.Entities;

public class TenantConfiguration : BaseEntity<string>
{
    public Guid TenantId { get; set; }
    public string PrimaryColor { get; set; } = "#007bff";
    public string SecondaryColor { get; set; } = "#6c757d";
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CustomCss { get; set; }
    public string Theme { get; set; } = "light";
    public Dictionary<string, object>? ThemeSettings { get; set; }
    public Dictionary<string, bool>? FeatureFlags { get; set; }
    public Dictionary<string, object>? CustomSettings { get; set; }
    public Dictionary<string, object>? BrandingSettings { get; set; }

    public static TenantConfiguration Default()
    {
        return new TenantConfiguration
        {
            PrimaryColor = "#007bff",
            SecondaryColor = "#6c757d",
            Theme = "light",
            ThemeSettings = new Dictionary<string, object>(),
            FeatureFlags = new Dictionary<string, bool>(),
            CustomSettings = new Dictionary<string, object>(),
            BrandingSettings = new Dictionary<string, object>()
        };
    }

    public static TenantConfiguration Create(Guid tenantId)
    {
        return new TenantConfiguration
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            PrimaryColor = "#007bff",
            SecondaryColor = "#6c757d",
            Theme = "light",
            ThemeSettings = new Dictionary<string, object>(),
            FeatureFlags = new Dictionary<string, bool>(),
            CustomSettings = new Dictionary<string, object>(),
            BrandingSettings = new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
