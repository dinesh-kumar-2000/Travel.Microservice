using PaymentService.Contracts.Responses.PaymentMethod;

namespace PaymentService.Contracts.Responses.PaymentMethod;

public class PaymentMethodListResponse
{
    public List<PaymentMethodResponse> PaymentMethods { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
