namespace NotificationService.Application.Queries.Sms;

public class GetSmsQuery
{
    public string TenantId { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
