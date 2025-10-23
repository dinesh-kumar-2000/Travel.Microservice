using NotificationService.Contracts.Responses.Email;

namespace NotificationService.Contracts.Responses.Email;

public class EmailHistoryResponse
{
    public List<EmailResponse> Emails { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class BulkEmailResponse
{
    public int TotalSent { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public List<string> FailedRecipients { get; set; } = new();
    public List<EmailResponse> EmailResponses { get; set; } = new();
}
