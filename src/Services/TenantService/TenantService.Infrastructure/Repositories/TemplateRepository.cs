using Dapper;
using TenantService.Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using SharedKernel.Utilities;
using Tenancy;

namespace TenantService.Infrastructure.Repositories;

public interface ITemplateRepository
{
    Task<EmailTemplate?> GetEmailTemplateByIdAsync(Guid id, Guid tenantId);
    Task<List<EmailTemplate>> GetEmailTemplatesAsync(Guid tenantId);
    Task<EmailTemplate> CreateEmailTemplateAsync(EmailTemplate template);
    Task<EmailTemplate?> UpdateEmailTemplateAsync(EmailTemplate template);
    Task<bool> DeleteEmailTemplateAsync(Guid id, Guid tenantId);

    Task<SMSTemplate?> GetSMSTemplateByIdAsync(Guid id, Guid tenantId);
    Task<List<SMSTemplate>> GetSMSTemplatesAsync(Guid tenantId);
    Task<SMSTemplate> CreateSMSTemplateAsync(SMSTemplate template);
    Task<SMSTemplate?> UpdateSMSTemplateAsync(SMSTemplate template);
    Task<bool> DeleteSMSTemplateAsync(Guid id, Guid tenantId);
}

public class TemplateRepository : ITemplateRepository
{
    private readonly IDapperContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TemplateRepository> _logger;

    public TemplateRepository(
        IDapperContext context,
        ITenantContext tenantContext,
        ILogger<TemplateRepository> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Email Templates

    public async Task<EmailTemplate?> GetEmailTemplateByIdAsync(Guid id, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   template_type AS TemplateType,
                   category AS Category,
                   subject AS Subject,
                   content AS Content,
                   variables AS Variables,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM email_templates
            WHERE id = @Id AND tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<EmailTemplate>(sql, new { Id = id, TenantId = tenantId });
    }

    public async Task<List<EmailTemplate>> GetEmailTemplatesAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   template_type AS TemplateType,
                   category AS Category,
                   subject AS Subject,
                   content AS Content,
                   variables AS Variables,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM email_templates
            WHERE tenant_id = @TenantId
            ORDER BY name ASC";

        var templates = await connection.QueryAsync<EmailTemplate>(sql, new { TenantId = tenantId });
        return templates.ToList();
    }

    public async Task<EmailTemplate> CreateEmailTemplateAsync(EmailTemplate template)
    {
        using var connection = _context.CreateConnection();
        
        template.Id = Guid.NewGuid();
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO email_templates (
                id, tenant_id, name, template_type, category, subject, content,
                variables, is_active, created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Name, @TemplateType, @Category, @Subject, @Content,
                @Variables, @IsActive, @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, template);

        _logger.LogInformation("Email template created: {Name}", template.Name);
        return template;
    }

    public async Task<EmailTemplate?> UpdateEmailTemplateAsync(EmailTemplate template)
    {
        using var connection = _context.CreateConnection();
        
        template.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE email_templates SET
                name = @Name,
                template_type = @TemplateType,
                category = @Category,
                subject = @Subject,
                content = @Content,
                variables = @Variables,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, template);
        return rowsAffected > 0 ? template : null;
    }

    public async Task<bool> DeleteEmailTemplateAsync(Guid id, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "DELETE FROM email_templates WHERE id = @Id AND tenant_id = @TenantId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TenantId = tenantId });

        return rowsAffected > 0;
    }

    #endregion

    #region SMS Templates

    public async Task<SMSTemplate?> GetSMSTemplateByIdAsync(Guid id, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   template_type AS TemplateType,
                   category AS Category,
                   content AS Content,
                   variables AS Variables,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM sms_templates
            WHERE id = @Id AND tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<SMSTemplate>(sql, new { Id = id, TenantId = tenantId });
    }

    public async Task<List<SMSTemplate>> GetSMSTemplatesAsync(Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   name AS Name,
                   template_type AS TemplateType,
                   category AS Category,
                   content AS Content,
                   variables AS Variables,
                   is_active AS IsActive,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM sms_templates
            WHERE tenant_id = @TenantId
            ORDER BY name ASC";

        var templates = await connection.QueryAsync<SMSTemplate>(sql, new { TenantId = tenantId });
        return templates.ToList();
    }

    public async Task<SMSTemplate> CreateSMSTemplateAsync(SMSTemplate template)
    {
        using var connection = _context.CreateConnection();
        
        template.Id = Guid.NewGuid();
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO sms_templates (
                id, tenant_id, name, template_type, category, content,
                variables, is_active, created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Name, @TemplateType, @Category, @Content,
                @Variables, @IsActive, @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, template);

        _logger.LogInformation("SMS template created: {Name}", template.Name);
        return template;
    }

    public async Task<SMSTemplate?> UpdateSMSTemplateAsync(SMSTemplate template)
    {
        using var connection = _context.CreateConnection();
        
        template.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE sms_templates SET
                name = @Name,
                template_type = @TemplateType,
                category = @Category,
                content = @Content,
                variables = @Variables,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, template);
        return rowsAffected > 0 ? template : null;
    }

    public async Task<bool> DeleteSMSTemplateAsync(Guid id, Guid tenantId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "DELETE FROM sms_templates WHERE id = @Id AND tenant_id = @TenantId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TenantId = tenantId });

        return rowsAffected > 0;
    }

    #endregion
}

