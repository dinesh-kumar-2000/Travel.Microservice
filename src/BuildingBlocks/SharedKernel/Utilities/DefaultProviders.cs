namespace SharedKernel.Utilities;

/// <summary>
/// Provides default singleton instances of common providers
/// Used in static contexts where dependency injection is not available
/// (e.g., DTOs, value objects, static methods)
/// </summary>
public static class DefaultProviders
{
    private static readonly Lazy<IIdGenerator> _lazyIdGenerator = 
        new(() => new UlidIdGenerator());
    
    private static readonly Lazy<IDateTimeProvider> _lazyDateTimeProvider = 
        new(() => new DateTimeProvider());

    /// <summary>
    /// Gets the default ID generator (ULID-based)
    /// </summary>
    public static IIdGenerator IdGenerator => _lazyIdGenerator.Value;

    /// <summary>
    /// Gets the default DateTime provider
    /// </summary>
    public static IDateTimeProvider DateTimeProvider => _lazyDateTimeProvider.Value;
}

