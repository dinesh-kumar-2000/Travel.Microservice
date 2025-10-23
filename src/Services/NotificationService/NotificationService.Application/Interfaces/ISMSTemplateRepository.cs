using NotificationService.Domain.Entities;
using SharedKernel.Data;

namespace NotificationService.Application.Interfaces;

public interface ISMSTemplateRepository : ITenantBaseRepository<SMSTemplate, string>
{
    Task<SMSTemplate?> GetByTemplateTypeAsync(string templateType, string tenantId);
    Task<List<SMSTemplate>> GetByCategoryAsync(string tenantId, string? category);
}

