using PaymentService.Contracts.Responses.Transaction;

namespace PaymentService.Contracts.Responses.Transaction;

public class TransactionHistoryResponse
{
    public List<TransactionResponse> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
