namespace BookingService.Domain.Saga;

public class BookingSagaState
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    
    public string BookingId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public int NumberOfTravelers { get; set; }
    public decimal Amount { get; set; }
    
    public string? PaymentId { get; set; }
    public bool InventoryReserved { get; set; }
    public bool PaymentProcessed { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
}

