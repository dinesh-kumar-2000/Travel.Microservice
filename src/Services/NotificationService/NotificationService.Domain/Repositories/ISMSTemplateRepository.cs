using NotificationService.Domain.Entities;
using SharedKernel.Data;

namespace NotificationService.Domain.Repositories;

public interface ISMSTemplateRepository : ITenantBaseRepository<SMSTemplate, string>
{
    Task<SMSTemplate?> GetByTemplateTypeAsync(string templateType, string tenantId);
    Task<List<SMSTemplate>> GetByCategoryAsync(string tenantId, string? category);
}

