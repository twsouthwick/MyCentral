using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client.Azure
{
    public class AzureServiceClient : IAsyncDisposable, IServiceClient, IObservable<Item>
    {
        private readonly ServiceClient _client;
        private readonly RegistryManager _registry;

        public string HostName { get; }

        public IObservable<Item> Events => this;

        public AzureServiceClient(string name, ServiceClient client, RegistryManager registry)
        {
            HostName = name;
            _client = client;
            _registry = registry;
        }

        public ValueTask DisposeAsync()
        {
            _client.Dispose();
            _registry.Dispose();

            return new ValueTask();
        }

        public async IAsyncEnumerable<Twin> GetDevicesAsync([EnumeratorCancellation] CancellationToken token)
        {
            var query = _registry.CreateQuery(@"select deviceId,
                              lastActivityTime,
                              connectionState,
                              status,
                              properties.reported.[[$iotin:deviceinfo]].manufacturer.value as manufacturer
                       from devices
                       where capabilities.iotEdge != true");

            while (query.HasMoreResults)
            {
                var result = await query.GetNextAsJsonAsync();

                foreach (var r in result)
                {
                    yield break;
                }
            }
        }

        IDisposable IObservable<Item>.Subscribe(IObserver<Item> observer)
            => new EmptyDisposable();

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
