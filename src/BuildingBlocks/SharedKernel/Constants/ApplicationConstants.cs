namespace SharedKernel.Constants;

/// <summary>
/// Centralized constants for the entire application
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Claim type names used across the application
    /// </summary>
    public static class Claims
    {
        public const string TenantId = "tenant_id";
        public const string UserId = "user_id";
    }

    /// <summary>
    /// HTTP header names used across the application
    /// </summary>
    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string TenantId = "X-Tenant-Id";
        public const string Authorization = "Authorization";
    }

    /// <summary>
    /// Event-related constants
    /// </summary>
    public static class Events
    {
        public const int DefaultRetryCount = 3;
        public const int DefaultRetryDelaySeconds = 5;
    }

    /// <summary>
    /// Queue configuration constants
    /// </summary>
    public static class Queue
    {
        public const int MessageTtlMilliseconds = 86400000; // 24 hours
        public const string QueueMode = "lazy";
    }
}

