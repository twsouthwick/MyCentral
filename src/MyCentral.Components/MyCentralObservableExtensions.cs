using MyCentral.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace MyCentral.Components
{
    public static class MyCentralObservableExtensions
    {
        public static IObservable<string> ObserveDeviceNames(this IServiceClient client)
        {
            return InitialList().ToObservable()
                .Merge(client.Events.Select(e => e.DeviceId))
                .Distinct();

            async IAsyncEnumerable<string> InitialList()
            {
                var devices = await client.GetDevicesAsync(default);

                foreach (var device in devices.Devices)
                {
                    yield return device.Name;
                }
            }
        }

        public static IObservable<Event> ObserveEventsFor(this IServiceClient client, string deviceId)
            => client.Events.Where(e => string.Equals(e.DeviceId, deviceId, StringComparison.Ordinal));
    }
}
