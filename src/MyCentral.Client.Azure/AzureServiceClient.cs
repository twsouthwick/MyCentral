using Azure.Core;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client.Azure
{
    public class AzureServiceClient : IAsyncDisposable, IServiceClient
    {
        private readonly ServiceClient _client;
        private readonly RegistryManager _registry;
        private readonly IoTHubOptions _options;

        public string HostName => _options.HostName;

        public IEventClient Events { get; }

        public AzureServiceClient(IOptions<IoTHubOptions> options, TokenCredential credential)
        {
            _options = options.Value;

            Events = new AzureEventClient(_options.EventHubConnectionString);
            _client = ServiceClient.Create(_options.HostName, credential);
            _registry = RegistryManager.Create(_options.HostName, credential);
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
    }
}
