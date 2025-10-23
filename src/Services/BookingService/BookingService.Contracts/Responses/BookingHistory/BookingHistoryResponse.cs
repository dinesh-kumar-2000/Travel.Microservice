namespace BookingService.Contracts.Responses.BookingHistory;

public class BookingHistoryResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int BookingType { get; set; }
    public decimal TotalAmount { get; set; }
    public int Status { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
