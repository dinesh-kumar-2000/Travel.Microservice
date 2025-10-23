using PaymentService.Application.DTOs.Responses.Transaction;

namespace PaymentService.Application.DTOs.Responses.Transaction;

public class TransactionHistoryResponse
{
    public List<TransactionResponse> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
