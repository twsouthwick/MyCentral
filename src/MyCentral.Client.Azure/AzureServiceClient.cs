using Azure.Core;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client.Azure
{
    public class AzureServiceClient : IAsyncDisposable, IServiceClient
    {
        private readonly ServiceClient _client;
        private readonly RegistryManager _registry;
        private readonly IoTHubOptions _options;
        private readonly DeviceCollection _devices;

        public string HostName => _options.HostName;

        public IEventClient Events { get; }

        public AzureServiceClient(IOptions<IoTHubOptions> options, IOptions<DeviceCollection> devices, TokenCredential credential)
        {
            _options = options.Value;
            _devices = devices.Value;

            Events = new AzureEventClient(_options.EventHubConnectionString);
            _client = ServiceClient.Create(_options.HostName, credential);
            _registry = RegistryManager.Create(_options.HostName, credential);
        }

        public async Task<string> InvokeMethodAsync(string deviceId, string methodName, string? payload = null)
        {
            var method = new CloudToDeviceMethod(methodName);

            if (payload is not null)
            {
                method.SetPayloadJson(payload);
            }

            var result = await _client.InvokeDeviceMethodAsync(deviceId, method);

            return result.GetPayloadAsJson();
        }

        public ValueTask DisposeAsync()
        {
            _client.Dispose();
            _registry.Dispose();

            return new ValueTask();
        }

        public async IAsyncEnumerable<string> GetDevicesAsync([EnumeratorCancellation] CancellationToken token)
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
                    var twin = JsonSerializer.Deserialize<Twin>(r);

                    if (twin.deviceId is not null)
                    {
                        yield return twin.deviceId;
                    }
                }
            }
        }

        private struct Twin
        {
            public string deviceId { get; set; }
        }
    }
}
