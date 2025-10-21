using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

public interface ISupportRepository
{
    // Tickets
    Task<SupportTicket?> GetTicketByIdAsync(Guid id, Guid userId);
    Task<List<SupportTicket>> GetUserTicketsAsync(Guid userId, string? status);
    Task<List<SupportTicket>> GetTenantTicketsAsync(Guid tenantId, string? status, string? priority, Guid? assignedTo, int page, int limit);
    Task<int> GetTenantTicketCountAsync(Guid tenantId, string? status, string? priority);
    Task<SupportTicket> CreateTicketAsync(SupportTicket ticket);
    Task<SupportTicket?> UpdateTicketAsync(SupportTicket ticket);
    
    // Messages
    Task<SupportMessage> AddMessageAsync(SupportMessage message);
    Task<List<SupportMessage>> GetTicketMessagesAsync(Guid ticketId);
    Task<bool> MarkMessageAsReadAsync(Guid messageId);
    
    // Tags
    Task<bool> AddTagAsync(SupportTicketTag tag);
    Task<bool> RemoveTagAsync(Guid ticketId, string tag);
    Task<List<string>> GetTicketTagsAsync(Guid ticketId);
    
    // Canned Responses
    Task<List<SupportCannedResponse>> GetCannedResponsesAsync(Guid tenantId, string? category);
    Task<SupportCannedResponse?> GetCannedResponseByIdAsync(Guid id);
    Task<SupportCannedResponse> CreateCannedResponseAsync(SupportCannedResponse response);
    Task<SupportCannedResponse?> UpdateCannedResponseAsync(SupportCannedResponse response);
    Task<bool> DeleteCannedResponseAsync(Guid id);
    
    // Activities
    Task<bool> LogActivityAsync(SupportTicketActivity activity);
    Task<List<SupportTicketActivity>> GetTicketActivitiesAsync(Guid ticketId);
    
    // SLA
    Task<SupportSLAConfig?> GetSLAConfigAsync(Guid tenantId, string priority);
}

