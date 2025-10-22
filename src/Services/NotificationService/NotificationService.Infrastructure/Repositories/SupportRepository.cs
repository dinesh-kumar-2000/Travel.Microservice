using Dapper;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using SharedKernel.Utilities;
using Tenancy;

namespace NotificationService.Infrastructure.Repositories;

public class SupportRepository : ISupportRepository
{
    private readonly IDapperContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SupportRepository> _logger;

    public SupportRepository(
        IDapperContext context,
        ITenantContext tenantContext,
        ILogger<SupportRepository> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<SupportTicket?> GetTicketByIdAsync(Guid id, Guid userId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   ticket_number AS TicketNumber,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   subject AS Subject,
                   category AS Category,
                   priority AS Priority,
                   status AS Status,
                   booking_id AS BookingId,
                   description AS Description,
                   assigned_to AS AssignedTo,
                   assigned_at AS AssignedAt,
                   resolved_at AS ResolvedAt,
                   closed_at AS ClosedAt,
                   resolution_notes AS ResolutionNotes,
                   satisfaction_rating AS SatisfactionRating,
                   satisfaction_feedback AS SatisfactionFeedback,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   last_response_at AS LastResponseAt
            FROM support_tickets
            WHERE id = @Id AND user_id = @UserId";

        return await connection.QueryFirstOrDefaultAsync<SupportTicket>(sql, 
            new { Id = id, UserId = userId });
    }

    public async Task<List<SupportTicket>> GetUserTicketsAsync(Guid userId, string? status)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string> { "user_id = @UserId" };
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", status);
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT id AS Id,
                   ticket_number AS TicketNumber,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   subject AS Subject,
                   category AS Category,
                   priority AS Priority,
                   status AS Status,
                   booking_id AS BookingId,
                   description AS Description,
                   assigned_to AS AssignedTo,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   last_response_at AS LastResponseAt
            FROM support_tickets
            WHERE {whereClause}
            ORDER BY created_at DESC";

        var tickets = await connection.QueryAsync<SupportTicket>(sql, parameters);
        return tickets.ToList();
    }

    public async Task<List<SupportTicket>> GetTenantTicketsAsync(
        Guid tenantId,
        string? status,
        string? priority,
        Guid? assignedTo,
        int page,
        int limit)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            whereClauses.Add("priority = @Priority");
            parameters.Add("Priority", priority);
        }

        if (assignedTo.HasValue)
        {
            whereClauses.Add("assigned_to = @AssignedTo");
            parameters.Add("AssignedTo", assignedTo.Value);
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var offset = (page - 1) * limit;

        var sql = $@"
            SELECT id AS Id,
                   ticket_number AS TicketNumber,
                   user_id AS UserId,
                   tenant_id AS TenantId,
                   subject AS Subject,
                   category AS Category,
                   priority AS Priority,
                   status AS Status,
                   booking_id AS BookingId,
                   assigned_to AS AssignedTo,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt,
                   last_response_at AS LastResponseAt
            FROM support_tickets
            WHERE {whereClause}
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset";

        parameters.Add("Limit", limit);
        parameters.Add("Offset", offset);

        var tickets = await connection.QueryAsync<SupportTicket>(sql, parameters);
        return tickets.ToList();
    }

    public async Task<int> GetTenantTicketCountAsync(Guid tenantId, string? status, string? priority)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(status))
        {
            whereClauses.Add("status = @Status");
            parameters.Add("Status", status);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            whereClauses.Add("priority = @Priority");
            parameters.Add("Priority", priority);
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var sql = $"SELECT COUNT(*) FROM support_tickets WHERE {whereClause}";

        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<SupportTicket> CreateTicketAsync(SupportTicket ticket)
    {
        using var connection = _context.CreateConnection();
        
        ticket.Id = Guid.NewGuid();
        ticket.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        ticket.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        // Ticket number is auto-generated by database trigger

        const string sql = @"
            INSERT INTO support_tickets (
                id, user_id, tenant_id, subject, category, priority, status,
                booking_id, description, created_at, updated_at
            )
            VALUES (
                @Id, @UserId, @TenantId, @Subject, @Category, @Priority, @Status,
                @BookingId, @Description, @CreatedAt, @UpdatedAt
            )
            RETURNING ticket_number";

        ticket.TicketNumber = await connection.ExecuteScalarAsync<string>(sql, ticket)
            ?? throw new InvalidOperationException("Failed to generate ticket number");

        _logger.LogInformation("Support ticket {TicketNumber} created", ticket.TicketNumber);
        return ticket;
    }

    public async Task<SupportTicket?> UpdateTicketAsync(SupportTicket ticket)
    {
        using var connection = _context.CreateConnection();
        
        ticket.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            UPDATE support_tickets SET
                status = @Status,
                assigned_to = @AssignedTo,
                assigned_at = @AssignedAt,
                resolved_at = @ResolvedAt,
                closed_at = @ClosedAt,
                resolution_notes = @ResolutionNotes,
                satisfaction_rating = @SatisfactionRating,
                satisfaction_feedback = @SatisfactionFeedback,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, ticket);
        return rowsAffected > 0 ? ticket : null;
    }

    public async Task<SupportMessage> AddMessageAsync(SupportMessage message)
    {
        using var connection = _context.CreateConnection();
        
        message.Id = Guid.NewGuid();
        message.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO support_messages (
                id, ticket_id, sender_id, sender_type, sender_name, content,
                attachments, is_internal_note, created_at
            )
            VALUES (
                @Id, @TicketId, @SenderId, @SenderType, @SenderName, @Content,
                @Attachments, @IsInternalNote, @CreatedAt
            )";

        await connection.ExecuteAsync(sql, message);
        return message;
    }

    public async Task<List<SupportMessage>> GetTicketMessagesAsync(Guid ticketId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   ticket_id AS TicketId,
                   sender_id AS SenderId,
                   sender_type AS SenderType,
                   sender_name AS SenderName,
                   content AS Content,
                   attachments AS Attachments,
                   is_internal_note AS IsInternalNote,
                   read_at AS ReadAt,
                   created_at AS CreatedAt
            FROM support_messages
            WHERE ticket_id = @TicketId
            ORDER BY created_at ASC";

        var messages = await connection.QueryAsync<SupportMessage>(sql, new { TicketId = ticketId });
        return messages.ToList();
    }

    public async Task<bool> MarkMessageAsReadAsync(Guid messageId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "UPDATE support_messages SET read_at = @ReadAt WHERE id = @Id AND read_at IS NULL";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = messageId, ReadAt = DefaultProviders.DateTimeProvider.UtcNow });

        return rowsAffected > 0;
    }

    public async Task<bool> AddTagAsync(SupportTicketTag tag)
    {
        using var connection = _context.CreateConnection();
        
        tag.Id = Guid.NewGuid();
        tag.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO support_ticket_tags (id, ticket_id, tag, created_at)
            VALUES (@Id, @TicketId, @Tag, @CreatedAt)
            ON CONFLICT (ticket_id, tag) DO NOTHING";

        var rowsAffected = await connection.ExecuteAsync(sql, tag);
        return rowsAffected > 0;
    }

    public async Task<bool> RemoveTagAsync(Guid ticketId, string tag)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "DELETE FROM support_ticket_tags WHERE ticket_id = @TicketId AND tag = @Tag";
        var rowsAffected = await connection.ExecuteAsync(sql, new { TicketId = ticketId, Tag = tag });

        return rowsAffected > 0;
    }

    public async Task<List<string>> GetTicketTagsAsync(Guid ticketId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "SELECT tag FROM support_ticket_tags WHERE ticket_id = @TicketId ORDER BY created_at";
        var tags = await connection.QueryAsync<string>(sql, new { TicketId = ticketId });

        return tags.ToList();
    }

    public async Task<List<SupportCannedResponse>> GetCannedResponsesAsync(Guid tenantId, string? category)
    {
        using var connection = _context.CreateConnection();
        
        var whereClauses = new List<string> { "tenant_id = @TenantId", "is_active = true" };
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        if (!string.IsNullOrEmpty(category))
        {
            whereClauses.Add("category = @Category");
            parameters.Add("Category", category);
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   content AS Content,
                   category AS Category,
                   shortcut AS Shortcut,
                   is_active AS IsActive,
                   usage_count AS UsageCount,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM support_canned_responses
            WHERE {whereClause}
            ORDER BY usage_count DESC, title ASC";

        var responses = await connection.QueryAsync<SupportCannedResponse>(sql, parameters);
        return responses.ToList();
    }

    public async Task<SupportCannedResponse?> GetCannedResponseByIdAsync(Guid id)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   title AS Title,
                   content AS Content,
                   category AS Category,
                   shortcut AS Shortcut,
                   is_active AS IsActive,
                   usage_count AS UsageCount,
                   created_at AS CreatedAt,
                   updated_at AS UpdatedAt
            FROM support_canned_responses
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<SupportCannedResponse>(sql, new { Id = id });
    }

    public async Task<SupportCannedResponse> CreateCannedResponseAsync(SupportCannedResponse response)
    {
        using var connection = _context.CreateConnection();
        
        response.Id = Guid.NewGuid();
        response.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;
        response.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO support_canned_responses (
                id, tenant_id, title, content, category, shortcut, is_active,
                usage_count, created_at, updated_at
            )
            VALUES (
                @Id, @TenantId, @Title, @Content, @Category, @Shortcut, @IsActive,
                @UsageCount, @CreatedAt, @UpdatedAt
            )";

        await connection.ExecuteAsync(sql, response);
        return response;
    }

    public async Task<SupportCannedResponse?> UpdateCannedResponseAsync(SupportCannedResponse response)
    {
        using var connection = _context.CreateConnection();
        
        response.UpdatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            UPDATE support_canned_responses SET
                title = @Title,
                content = @Content,
                category = @Category,
                shortcut = @Shortcut,
                is_active = @IsActive,
                usage_count = @UsageCount,
                updated_at = @UpdatedAt
            WHERE id = @Id AND tenant_id = @TenantId";

        var rowsAffected = await connection.ExecuteAsync(sql, response);
        return rowsAffected > 0 ? response : null;
    }

    public async Task<bool> DeleteCannedResponseAsync(Guid id)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = "DELETE FROM support_canned_responses WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    public async Task<bool> LogActivityAsync(SupportTicketActivity activity)
    {
        using var connection = _context.CreateConnection();
        
        activity.Id = Guid.NewGuid();
        activity.CreatedAt = DefaultProviders.DateTimeProvider.UtcNow;

        const string sql = @"
            INSERT INTO support_ticket_activities (
                id, ticket_id, user_id, activity_type, old_value, new_value,
                description, created_at
            )
            VALUES (
                @Id, @TicketId, @UserId, @ActivityType, @OldValue, @NewValue,
                @Description, @CreatedAt
            )";

        var rowsAffected = await connection.ExecuteAsync(sql, activity);
        return rowsAffected > 0;
    }

    public async Task<List<SupportTicketActivity>> GetTicketActivitiesAsync(Guid ticketId)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   ticket_id AS TicketId,
                   user_id AS UserId,
                   activity_type AS ActivityType,
                   old_value AS OldValue,
                   new_value AS NewValue,
                   description AS Description,
                   created_at AS CreatedAt
            FROM support_ticket_activities
            WHERE ticket_id = @TicketId
            ORDER BY created_at ASC";

        var activities = await connection.QueryAsync<SupportTicketActivity>(sql, new { TicketId = ticketId });
        return activities.ToList();
    }

    public async Task<SupportSLAConfig?> GetSLAConfigAsync(Guid tenantId, string priority)
    {
        using var connection = _context.CreateConnection();
        
        const string sql = @"
            SELECT id AS Id,
                   tenant_id AS TenantId,
                   priority AS Priority,
                   first_response_time_minutes AS FirstResponseTimeMinutes,
                   resolution_time_hours AS ResolutionTimeHours,
                   created_at AS CreatedAt
            FROM support_sla_config
            WHERE tenant_id = @TenantId AND priority = @Priority";

        return await connection.QueryFirstOrDefaultAsync<SupportSLAConfig>(sql, 
            new { TenantId = tenantId, Priority = priority });
    }
}

