using System.Threading.RateLimiting;
using Fleck;

namespace Backend.service;

public class WsWithMetaData(IWebSocketConnection connection)
{
    public IWebSocketConnection Connection { get; set; } = connection;
    public bool IsAuthenticated { get; set; } = false;
    
}

public static class StateService
{
    private static readonly Dictionary<Guid, WsWithMetaData> _connections = new();

    public static void AddConnection(Guid clientId, IWebSocketConnection ws)
    {
        _connections.TryAdd(clientId, new WsWithMetaData(ws));
    }


    public static void RemoveConnection(Guid clientId)
    {
        _connections.Remove(clientId);
    }
   
}