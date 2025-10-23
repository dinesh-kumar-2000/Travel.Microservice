using CmsService.Domain.Entities;
using SharedKernel.Data;

namespace CmsService.Application.Interfaces;

public interface ITemplateRepository : IBaseRepository<CmsTemplate, string>
{
    Task<IEnumerable<CmsTemplate>> GetActiveTemplatesAsync();
    Task<CmsTemplate?> GetByTemplateTypeAsync(string templateType);
}
