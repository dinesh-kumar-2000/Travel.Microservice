namespace SharedKernel.Utilities;

/// <summary>
/// Abstraction for generating unique identifiers
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// Generates a new unique identifier as a string
    /// </summary>
    string Generate();
}

/// <summary>
/// ULID-based ID generator (recommended for distributed systems)
/// </summary>
public class UlidIdGenerator : IIdGenerator
{
    public string Generate() => Ulid.NewUlid().ToString();
}

/// <summary>
/// GUID-based ID generator (fallback for compatibility)
/// </summary>
public class GuidIdGenerator : IIdGenerator
{
    public string Generate() => Guid.NewGuid().ToString();
}

