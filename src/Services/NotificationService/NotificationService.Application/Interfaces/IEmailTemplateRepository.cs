using NotificationService.Domain.Entities;
using SharedKernel.Data;

namespace NotificationService.Application.Interfaces;

public interface IEmailTemplateRepository : ITenantBaseRepository<EmailTemplate, string>
{
    Task<EmailTemplate?> GetByTemplateTypeAsync(string templateType, string tenantId);
    Task<List<EmailTemplate>> GetByCategoryAsync(string tenantId, string? category);
}

