namespace NotificationService.Application.Queries.Email;

public class GetEmailsQuery
{
    public string TenantId { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
