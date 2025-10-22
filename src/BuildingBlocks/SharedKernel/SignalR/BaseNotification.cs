using SharedKernel.Utilities;

namespace SharedKernel.SignalR;

/// <summary>
/// Base class for all notification types to eliminate timestamp duplication
/// </summary>
public abstract class BaseNotification
{
    public DateTime Timestamp { get; set; } = DefaultProviders.DateTimeProvider.UtcNow;
    public string Message { get; set; } = string.Empty;
}

