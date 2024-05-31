using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using Fleck;

namespace Backend.service;

public class WsWithMetaData
{
    public int UserId { get; set; }
    public bool IsAuthenticated { get; set; }
}

public static class StateService
{
    private static readonly ConcurrentDictionary<IWebSocketConnection, WsWithMetaData> _connections = new();

    public static void AddConnection(IWebSocketConnection ws)
    {
        _connections.TryAdd(ws, new WsWithMetaData());
    }


    public static WsWithMetaData GetMetaData(IWebSocketConnection ws)
    {
        return _connections[ws];
    }


    public static void RemoveConnection(IWebSocketConnection ws)
    {
        _connections.TryRemove(ws, out _);
    }

    public static void AuthenticateUser(IWebSocketConnection ws, int userId)
    {
        if (_connections.TryGetValue(ws, out var wsWithMetaData))
        {
            if (wsWithMetaData.UserId == userId)
            {
                wsWithMetaData.IsAuthenticated = true;
            }
            else
            {
                throw new Exception("UserId does not match");
            }
        }
        else
        {
            throw new Exception("Connection not found");
        }
    }
    
    public static void SetUserId(IWebSocketConnection ws, int userId)
    {
        if (_connections.TryGetValue(ws, out var wsWithMetaData))
        {
            wsWithMetaData.UserId = userId;
        }
        else
        {
            throw new Exception("Connection not found");
        }
    }
}