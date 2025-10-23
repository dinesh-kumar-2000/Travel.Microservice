using CmsService.Domain.Entities;
using CmsService.Application.Interfaces;
using SharedKernel.Data;

namespace CmsService.Infrastructure.Persistence.Repositories;

public class TemplateRepository : BaseRepository<CmsTemplate>, ITemplateRepository
{
    public TemplateRepository(DapperContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CmsTemplate>> GetActiveTemplatesAsync()
    {
        const string sql = @"
            SELECT * FROM cms_templates 
            WHERE is_active = true AND is_deleted = false 
            ORDER BY name";
        
        return await QueryAsync<CmsTemplate>(sql);
    }

    public async Task<CmsTemplate?> GetByTemplateTypeAsync(string templateType)
    {
        const string sql = @"
            SELECT * FROM cms_templates 
            WHERE template_type = @TemplateType AND is_active = true AND is_deleted = false";
        
        return await QueryFirstOrDefaultAsync<CmsTemplate>(sql, new { TemplateType = templateType });
    }
}
