using System.Text.RegularExpressions;

namespace TenantService.Domain.ValueObjects;

public class Subdomain
{
    public string Value { get; private set; }

    private Subdomain() { Value = string.Empty; }

    public Subdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain cannot be null or empty", nameof(subdomain));

        if (!IsValidSubdomain(subdomain))
            throw new ArgumentException("Invalid subdomain format", nameof(subdomain));

        Value = subdomain.ToLowerInvariant();
    }

    private static bool IsValidSubdomain(string subdomain)
    {
        // Subdomain validation: 3-63 characters, alphanumeric and hyphens, not starting/ending with hyphen
        var pattern = @"^[a-zA-Z0-9]([a-zA-Z0-9-]{1,61}[a-zA-Z0-9])?$";
        return Regex.IsMatch(subdomain, pattern) && subdomain.Length >= 3 && subdomain.Length <= 63;
    }

    public static implicit operator string(Subdomain subdomain) => subdomain.Value;
    public static implicit operator Subdomain(string subdomain) => new(subdomain);

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Subdomain subdomain && Value == subdomain.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
