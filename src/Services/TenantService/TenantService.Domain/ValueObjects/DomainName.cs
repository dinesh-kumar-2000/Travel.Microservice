using System.Text.RegularExpressions;

namespace TenantService.Domain.ValueObjects;

public class DomainName
{
    public string Value { get; private set; }

    private DomainName() { Value = string.Empty; }

    public DomainName(string domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

        if (!IsValidDomainName(domainName))
            throw new ArgumentException("Invalid domain name format", nameof(domainName));

        Value = domainName.ToLowerInvariant();
    }

    private static bool IsValidDomainName(string domainName)
    {
        // Domain name validation: RFC 1123 compliant
        var pattern = @"^[a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        return Regex.IsMatch(domainName, pattern) && domainName.Length <= 253;
    }

    public static implicit operator string(DomainName domainName) => domainName.Value;
    public static implicit operator DomainName(string domainName) => new(domainName);

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is DomainName domainName && Value == domainName.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
