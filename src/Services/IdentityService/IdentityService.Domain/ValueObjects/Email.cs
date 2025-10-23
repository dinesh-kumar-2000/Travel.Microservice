using System.ComponentModel.DataAnnotations;

namespace IdentityService.Domain.ValueObjects;

public class Email
{
    public string Value { get; private set; }

    private Email() { Value = string.Empty; }

    public Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        if (!new EmailAddressAttribute().IsValid(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        Value = email.ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Email email && Value == email.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
