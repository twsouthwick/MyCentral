using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCentral.Client.Azure
{
    public class AzureEventClient : IEventClient
    {
        private readonly EventHubConsumerClient _events;
        private readonly IObservable<Event> _observable;

        public AzureEventClient(string eventConnectionString)
        {
            _events = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, eventConnectionString);
            _observable = _events.ReadEventsAsync()
                .Select(t => new Event(GetDeviceId(t.Data.SystemProperties), t.Data.EnqueuedTime, t.Data.EventBody.ToString(), t.Data.Properties, t.Data.SystemProperties))
                .ToObservable();
        }

        private static string GetDeviceId(IReadOnlyDictionary<string, object> properties)
        {
            if (properties.TryGetValue("iothub-connection-device-id", out var deviceId))
            {
                return deviceId?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        public ValueTask DisposeAsync()
            => _events.DisposeAsync();

        public IDisposable Subscribe(IObserver<Event> observer)
            => _observable.Subscribe(observer);
    }
}
