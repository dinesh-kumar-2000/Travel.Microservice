using PaymentService.Application.DTOs.Responses.PaymentMethod;

namespace PaymentService.Application.DTOs.Responses.PaymentMethod;

public class PaymentMethodListResponse
{
    public List<PaymentMethodResponse> PaymentMethods { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
