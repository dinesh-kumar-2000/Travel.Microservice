using SharedKernel.Models;

namespace PaymentService.Domain.Entities;

public class PaymentMethod : BaseEntity<string>
{
    public Guid UserId { get; set; }
    public int PaymentMethodType { get; set; }
    public string MaskedCardNumber { get; set; } = string.Empty;
    public string EncryptedCardNumber { get; set; } = string.Empty;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
