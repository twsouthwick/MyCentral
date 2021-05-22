using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DeviceService.Server.Hubs
{
    public class EventHubConnections
    {
        private readonly ConcurrentDictionary<string, string> _map = new();
        private readonly ReferenceCountingCollection<string> _cache = new();

        public void AddHostName(string id, string hostname)
        {
            if (!_map.TryAdd(id, hostname))
            {
                _map.TryRemove(id, out _);
                _map.TryAdd(id, hostname);
            }

            _cache.Add(hostname);
        }

        public bool TryRemove(string id, out string hostname)
        {
            if (_map.TryRemove(id, out hostname))
            {
                _cache.Remove(hostname);
                return true;
            }

            return false;
        }

        public IEnumerable<string> HostNames => _cache;
    }
}
