namespace SharedKernel.SignalR;

/// <summary>
/// Centralized SignalR group name builder to eliminate string duplication
/// </summary>
public static class SignalRGroupNames
{
    /// <summary>
    /// Gets the group name for a specific user
    /// </summary>
    public static string ForUser(string userId) => $"user:{userId}";

    /// <summary>
    /// Gets the group name for a specific tenant
    /// </summary>
    public static string ForTenant(string tenantId) => $"tenant:{tenantId}";

    /// <summary>
    /// Gets the group name for a specific notification type
    /// </summary>
    public static string ForNotificationType(string notificationType) => $"notifications:{notificationType}";

    /// <summary>
    /// Gets the group name for a custom group
    /// </summary>
    public static string ForCustomGroup(string groupName) => groupName;
}

