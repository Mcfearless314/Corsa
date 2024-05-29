using Fleck;
using lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.exceptions;

namespace Backend.EventFilters
{
    public class RateLimiterAttribute : BaseEventFilter
    {
        private static readonly Dictionary<Guid, Queue<DateTime>> RequestTimestamps = new();
        private readonly int _eventsPerTimeFrame;
        private readonly TimeSpan _timeFrame;
        private readonly int _eventsPerShortTimeFrame;
        private readonly TimeSpan _shortTimeFrame;

        public RateLimiterAttribute(int eventsPerTimeFrame, int timeFrameInMinutes, int eventsPerShortTimeFrame, int shortTimeFrameInMinutes)
        {
            _eventsPerTimeFrame = eventsPerTimeFrame;
            _timeFrame = TimeSpan.FromMinutes(timeFrameInMinutes);
            _eventsPerShortTimeFrame = eventsPerShortTimeFrame;
            _shortTimeFrame = TimeSpan.FromMinutes(shortTimeFrameInMinutes);
        }

        public override Task Handle<T>(IWebSocketConnection socket, T dto)
        {
            var clientId = socket.ConnectionInfo.Id;
            if (!RequestTimestamps.ContainsKey(clientId))
            {
                RequestTimestamps[clientId] = new Queue<DateTime>();
            }

            var timestamps = RequestTimestamps[clientId];
            var now = DateTime.UtcNow;

            // Remove timestamps older than _timeFrame
            while (timestamps.Count > 0 && now - timestamps.Peek() > _timeFrame)
            {
                timestamps.Dequeue();
            }

            // Check if the client has sent more than _eventsPerTimeFrame in the last _timeFrame
            if (timestamps.Count >= _eventsPerTimeFrame)
            {
                throw new TooManyRequestsException($"Rate limit exceeded: More than {_eventsPerTimeFrame} requests in the last {_timeFrame.TotalMinutes} minutes");
            }

            // Check if the client has sent more than _eventsPerShortTimeFrame in the last _shortTimeFrame
            if (timestamps.Count(t => now - t <= _shortTimeFrame) >= _eventsPerShortTimeFrame)
            {
                throw new TooManyRequestsException($"Rate limit exceeded: More than {_eventsPerShortTimeFrame} requests in the last {_shortTimeFrame.TotalMinutes} minutes");
            }

            // Add the current timestamp to the queue
            timestamps.Enqueue(now);

            return Task.CompletedTask;
        }
    }
}