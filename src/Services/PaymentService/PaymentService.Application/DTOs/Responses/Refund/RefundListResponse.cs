using PaymentService.Application.DTOs.Responses.Refund;

namespace PaymentService.Application.DTOs.Responses.Refund;

public class RefundListResponse
{
    public List<RefundResponse> Refunds { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
