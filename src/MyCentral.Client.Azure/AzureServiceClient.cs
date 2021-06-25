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
                       from devices
                       where capabilities.iotEdge != true");

            while (query.HasMoreResults)
            {
                var result = await query.GetNextAsJsonAsync();

                foreach (var r in result)
                {
                    var twin = System.Text.Json.JsonSerializer.Deserialize<DeviceTwin>(r);

                    if (twin.deviceId is not null)
                    {
                        yield return twin.deviceId;
                    }
                }
            }
        }

        public async Task UpdatePropertyAsync(string deviceId, string componentName, string propertyName, string propertyValue)
        {
            var patch = CreatePropertyPatch(propertyName, propertyValue, componentName);
            var twin = await _registry.GetTwinAsync(deviceId);
            await _registry.UpdateTwinAsync(deviceId, patch, twin.ETag);
        }

        /* The property update patch (for a property within a component) needs to be in the following format:
         * {
         *  "sampleComponentName":
         *      {
         *          "__t": "c",
         *          "samplePropertyName": 20
         *      }
         *  }
         */
        private static Twin CreatePropertyPatch(string propertyName, string propertyValue, string componentName)
        {
            var twinPatch = new Twin();
            twinPatch.Properties.Desired[componentName] = new
            {
                __t = "c"
            };
            twinPatch.Properties.Desired[componentName][propertyName] = propertyValue;
            return twinPatch;
        }

        private struct DeviceTwin
        {
            public string deviceId { get; set; }
        }
    }
}
