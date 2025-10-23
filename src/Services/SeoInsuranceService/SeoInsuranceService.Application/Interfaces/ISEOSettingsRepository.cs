using SeoInsuranceService.Domain.Entities;
using SharedKernel.Data;

namespace SeoInsuranceService.Application.Interfaces;

public interface ISEOSettingsRepository : ITenantBaseRepository<SEOSettings, string>
{
    Task<bool> GenerateSitemapAsync(string tenantId);
}

