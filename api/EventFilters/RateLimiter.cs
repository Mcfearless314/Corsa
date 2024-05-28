using Fleck;
using lib;

namespace Backend.EventFilters;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RateLimiterAttribute(int eventsPerTimeframe, int secondTimeFrame) : BaseEventFilter
{
    public override Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        //TODO implement rate limiter
        return Task.CompletedTask;
    }
}