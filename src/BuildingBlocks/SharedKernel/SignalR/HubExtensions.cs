using Microsoft.AspNetCore.SignalR;

namespace SharedKernel.SignalR;

public static class HubExtensions
{
    public static async Task SendToUserAsync<T>(this IHubContext<T> hubContext, string userId, string method, object? arg1 = null, object? arg2 = null, object? arg3 = null, object? arg4 = null, object? arg5 = null, object? arg6 = null, object? arg7 = null, object? arg8 = null, object? arg9 = null, object? arg10 = null) where T : Hub
    {
        await hubContext.Clients.User(userId).SendAsync(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }

    public static async Task SendToGroupAsync<T>(this IHubContext<T> hubContext, string groupName, string method, object? arg1 = null, object? arg2 = null, object? arg3 = null, object? arg4 = null, object? arg5 = null, object? arg6 = null, object? arg7 = null, object? arg8 = null, object? arg9 = null, object? arg10 = null) where T : Hub
    {
        await hubContext.Clients.Group(groupName).SendAsync(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }

    public static async Task SendToAllAsync<T>(this IHubContext<T> hubContext, string method, object? arg1 = null, object? arg2 = null, object? arg3 = null, object? arg4 = null, object? arg5 = null, object? arg6 = null, object? arg7 = null, object? arg8 = null, object? arg9 = null, object? arg10 = null) where T : Hub
    {
        await hubContext.Clients.All.SendAsync(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }

    public static async Task SendToAllExceptAsync<T>(this IHubContext<T> hubContext, string method, IReadOnlyList<string> excludedConnectionIds, object? arg1 = null, object? arg2 = null, object? arg3 = null, object? arg4 = null, object? arg5 = null, object? arg6 = null, object? arg7 = null, object? arg8 = null, object? arg9 = null, object? arg10 = null) where T : Hub
    {
        await hubContext.Clients.AllExcept(excludedConnectionIds).SendAsync(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }

    public static async Task SendToConnectionAsync<T>(this IHubContext<T> hubContext, string connectionId, string method, object? arg1 = null, object? arg2 = null, object? arg3 = null, object? arg4 = null, object? arg5 = null, object? arg6 = null, object? arg7 = null, object? arg8 = null, object? arg9 = null, object? arg10 = null) where T : Hub
    {
        await hubContext.Clients.Client(connectionId).SendAsync(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
    }
}
