namespace SharedKernel.Utilities;

/// <summary>
/// Legacy static ULID generator - kept for backward compatibility
/// For new code, use IIdGenerator abstraction instead
/// </summary>
[Obsolete("Use IIdGenerator abstraction (UlidIdGenerator or GuidIdGenerator) instead")]
public static class UlidGenerator
{
    public static string Generate() => Ulid.NewUlid().ToString();
    
    public static bool TryParse(string value, out Ulid ulid)
    {
        return Ulid.TryParse(value, out ulid);
    }
}

