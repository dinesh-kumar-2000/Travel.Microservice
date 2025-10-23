using SeoInsuranceService.Domain.Entities;
using SharedKernel.Data;

namespace SeoInsuranceService.Domain.Repositories;

public interface ISEOSettingsRepository : ITenantBaseRepository<SEOSettings, string>
{
    Task<bool> GenerateSitemapAsync(string tenantId);
}

