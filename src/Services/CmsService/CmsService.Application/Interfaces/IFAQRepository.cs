using CmsService.Domain.Entities;
using SharedKernel.Data;

namespace CmsService.Application.Interfaces;

public interface IFAQRepository : ITenantBaseRepository<FAQ, string>
{
    Task<List<FAQ>> GetByCategoryAsync(string tenantId, string? category);
}

