using MyCentral.Client;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace MyCentral.Components
{
    public static class MyCentralObservableExtensions
    {
        public static IObservable<string> ObserveDeviceNames(this IServiceClient client)
        {
            return client.GetDevicesAsync(default).ToObservable()
                .Merge(client.Events.Select(e => e.DeviceId))
                .Distinct();
        }

        public static IObservable<Event> ObserveEventsFor(this IServiceClient client, string deviceId)
            => client.Events.Where(e => string.Equals(e.DeviceId, deviceId, StringComparison.Ordinal));
    }
}
