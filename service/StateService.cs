using System.Threading.RateLimiting;
using Fleck;

namespace Backend.service;

public class WsWithMetaData(IWebSocketConnection connection)
{
    public IWebSocketConnection Connection { get; set; } = connection;

    public Dictionary<string, RateLimiter> RateLimitPerEvent { get; set; } = new();
}

public static class StateService
{
    private static Dictionary<Guid, IWebSocketConnection> _connections = new();

    public static bool AddConnection(IWebSocketConnection ws)
    {
        return _connections.TryAdd(ws.ConnectionInfo.Id, ws);
    }


    public static bool RemoveConnection(IWebSocketConnection ws)
    {
        return _connections.Remove(ws.ConnectionInfo.Id);
    }
}