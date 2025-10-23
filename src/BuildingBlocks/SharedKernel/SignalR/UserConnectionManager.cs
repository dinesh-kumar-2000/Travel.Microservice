using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace SharedKernel.SignalR;

public interface IUserConnectionManager
{
    void AddConnection(string userId, string connectionId);
    void RemoveConnection(string userId, string connectionId);
    IReadOnlyList<string> GetUserConnections(string userId);
    IReadOnlyList<string> GetAllConnectedUsers();
    bool IsUserConnected(string userId);
    int GetConnectionCount(string userId);
    void ClearUserConnections(string userId);
}

public class UserConnectionManager : IUserConnectionManager
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();
    private readonly ILogger<UserConnectionManager> _logger;

    public UserConnectionManager(ILogger<UserConnectionManager> logger)
    {
        _logger = logger;
    }

    public void AddConnection(string userId, string connectionId)
    {
        _userConnections.AddOrUpdate(
            userId,
            new HashSet<string> { connectionId },
            (key, existingConnections) =>
            {
                existingConnections.Add(connectionId);
                return existingConnections;
            });

        _logger.LogInformation("Added connection {ConnectionId} for user {UserId}. Total connections: {Count}", 
            connectionId, userId, GetConnectionCount(userId));
    }

    public void RemoveConnection(string userId, string connectionId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            connections.Remove(connectionId);
            
            if (connections.Count == 0)
            {
                _userConnections.TryRemove(userId, out _);
                _logger.LogInformation("Removed last connection for user {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("Removed connection {ConnectionId} for user {UserId}. Remaining connections: {Count}", 
                    connectionId, userId, connections.Count);
            }
        }
    }

    public IReadOnlyList<string> GetUserConnections(string userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            return connections.ToList().AsReadOnly();
        }
        
        return new List<string>().AsReadOnly();
    }

    public IReadOnlyList<string> GetAllConnectedUsers()
    {
        return _userConnections.Keys.ToList().AsReadOnly();
    }

    public bool IsUserConnected(string userId)
    {
        return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
    }

    public int GetConnectionCount(string userId)
    {
        return _userConnections.TryGetValue(userId, out var connections) ? connections.Count : 0;
    }

    public void ClearUserConnections(string userId)
    {
        if (_userConnections.TryRemove(userId, out var connections))
        {
            _logger.LogInformation("Cleared all {Count} connections for user {UserId}", connections.Count, userId);
        }
    }
}
