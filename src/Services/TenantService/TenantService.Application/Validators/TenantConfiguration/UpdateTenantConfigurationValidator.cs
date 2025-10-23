using FluentValidation;
using TenantService.Application.Commands.TenantConfiguration;

namespace TenantService.Application.Validators.TenantConfiguration;

public class UpdateTenantConfigurationValidator : AbstractValidator<UpdateTenantConfigurationCommand>
{
    public UpdateTenantConfigurationValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");

        RuleFor(x => x.ThemeSettings)
            .Must(BeValidJsonObject).WithMessage("Theme settings must be valid JSON")
            .When(x => x.ThemeSettings != null);

        RuleFor(x => x.FeatureFlags)
            .Must(BeValidFeatureFlags).WithMessage("Feature flags must contain valid boolean values")
            .When(x => x.FeatureFlags != null);

        RuleFor(x => x.CustomSettings)
            .Must(BeValidJsonObject).WithMessage("Custom settings must be valid JSON")
            .When(x => x.CustomSettings != null);

        RuleFor(x => x.BrandingSettings)
            .Must(BeValidJsonObject).WithMessage("Branding settings must be valid JSON")
            .When(x => x.BrandingSettings != null);
    }

    private static bool BeValidJsonObject(Dictionary<string, object>? settings)
    {
        if (settings == null) return true;
        
        try
        {
            // Basic validation - could be enhanced with more specific rules
            return settings.All(kvp => !string.IsNullOrEmpty(kvp.Key));
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidFeatureFlags(Dictionary<string, bool>? flags)
    {
        if (flags == null) return true;
        
        try
        {
            return flags.All(kvp => !string.IsNullOrEmpty(kvp.Key));
        }
        catch
        {
            return false;
        }
    }
}
