namespace PaymentService.Application.DTOs.Responses.PaymentMethod;

public class PaymentMethodResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int PaymentMethodType { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
