using Azure.Core;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AzureServiceClient> _logger;

        public string HostName => _options.HostName;

        public IEventClient Events { get; }

        public AzureServiceClient(
            IOptions<IoTHubOptions> options,
            TokenCredential credential,
            ILogger<AzureServiceClient> logger)
        {
            _options = options.Value;
            _logger = logger;

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
                var result = default(IEnumerable<string>);

                try
                {
                    result = await query.GetNextAsJsonAsync();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error getting device info from {HostName}", HostName);
                }

                if (result is null)
                {
                    yield break;
                }

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
