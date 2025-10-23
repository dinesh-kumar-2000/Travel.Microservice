using CmsService.Domain.Entities;
using SharedKernel.Data;

namespace CmsService.Domain.Repositories;

public interface IFAQRepository : ITenantBaseRepository<FAQ, string>
{
    Task<List<FAQ>> GetByCategoryAsync(string tenantId, string? category);
}

