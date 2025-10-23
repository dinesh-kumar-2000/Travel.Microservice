using NotificationService.Domain.Entities;
using SharedKernel.Data;

namespace NotificationService.Domain.Repositories;

public interface ISupportTicketRepository : ITenantBaseRepository<SupportTicket, string>
{
    Task<SupportTicket?> GetByIdForUserAsync(string id, string userId);
    Task<List<SupportTicket>> GetByUserIdAsync(string userId, string? status);
    Task<List<SupportTicket>> GetByTenantWithFiltersAsync(
        string tenantId, 
        string? status, 
        string? priority, 
        string? assignedTo, 
        int page, 
        int limit);
    Task<int> GetCountByTenantWithFiltersAsync(
        string tenantId, 
        string? status, 
        string? priority);
}

public interface ISupportMessageRepository : IBaseRepository<SupportMessage, string>
{
    Task<List<SupportMessage>> GetByTicketIdAsync(string ticketId);
    Task<bool> MarkAsReadAsync(string messageId);
}

public interface ISupportTicketTagRepository : IBaseRepository<SupportTicketTag, string>
{
    Task<List<string>> GetTagsByTicketIdAsync(string ticketId);
    Task<bool> RemoveByTicketAndTagAsync(string ticketId, string tag);
}

public interface ISupportCannedResponseRepository : ITenantBaseRepository<SupportCannedResponse, string>
{
    Task<List<SupportCannedResponse>> GetByCategoryAsync(string tenantId, string? category);
}

public interface ISupportSLAConfigRepository : ITenantBaseRepository<SupportSLAConfig, string>
{
    Task<SupportSLAConfig?> GetByPriorityAsync(string tenantId, string priority);
}

public interface ISupportTicketActivityRepository : IBaseRepository<SupportTicketActivity, string>
{
    Task<List<SupportTicketActivity>> GetByTicketIdAsync(string ticketId);
}

