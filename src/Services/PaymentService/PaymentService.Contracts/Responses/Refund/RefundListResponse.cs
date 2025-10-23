using PaymentService.Contracts.Responses.Refund;

namespace PaymentService.Contracts.Responses.Refund;

public class RefundListResponse
{
    public List<RefundResponse> Refunds { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
