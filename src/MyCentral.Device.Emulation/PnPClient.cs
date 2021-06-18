using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public delegate void OnDesiredPropertyFoundCallback(TwinCollection newValue);

    public sealed class PnPClient : IAsyncDisposable, IEmulatedDevice
    {
        private readonly DeviceClient deviceClient;
        private readonly Dictionary<string, OnDesiredPropertyFoundCallback> desiredPropertyCallbacks = new Dictionary<string, OnDesiredPropertyFoundCallback>();

        public PnPClient(DeviceClient client)
        {
            deviceClient = client;
            deviceClient.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback, deviceClient);
        }

        public Task SendAsync(string componentName, byte[] bytes)
        {
            var message = new Message(bytes)
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8"
            };

            message.Properties.Add("$.sub", componentName);

            return deviceClient.SendEventAsync(message);
        }

        public void SetDesiredPropertyUpdateCommandHandler(string componentName, OnDesiredPropertyFoundCallback callback)
        {
            desiredPropertyCallbacks.Add(componentName, callback);
        }

        public async Task ReportComponentPropertyCollectionAsync(string componentName, Dictionary<string, object> properties)
        {
            var reported = new TwinCollection();
            foreach (var p in properties)
            {
                reported.AddComponentProperty(componentName, p.Key, p.Value);
            }
            await deviceClient.UpdateReportedPropertiesAsync(reported);
        }

        public async Task ReportPropertyAsync(string propertyName, object propertyValue)
        {
            var twin = new TwinCollection();
            twin[propertyName] = propertyValue;
            await deviceClient.UpdateReportedPropertiesAsync(twin);
        }

        public async Task ReportComponentPropertyAsync(string componentName, string propertyName, object propertyValue)
        {
            var twin = new TwinCollection();
            twin.AddComponentProperty(componentName, propertyName, propertyValue);
            await deviceClient.UpdateReportedPropertiesAsync(twin);
        }

        public async Task SetCommandHandlerAsync(string commandName, MethodCallback callback, object? ctx)
        {
            await deviceClient.SetMethodHandlerAsync($"{commandName}", callback, ctx);
        }

        public async Task SetComponentCommandHandlerAsync(string componentName, string commandName, MethodCallback callback, object? ctx)
        {
            await deviceClient.SetMethodHandlerAsync($"{componentName}*{commandName}", callback, ctx);
        }

        public async Task<T?> ReadDesiredComponentPropertyAsync<T>(string componentName, string propertyName)
        {
            var twin = await deviceClient.GetTwinAsync();
            var desiredPropertyValue = twin.Properties.Desired.GetPropertyValue<T>(componentName, propertyName);
            if (Comparer<T>.Default.Compare(desiredPropertyValue, default) > 0)
            {
                await AckDesiredPropertyReadAsync(componentName, propertyName, desiredPropertyValue!, StatusCodes.Completed, "update complete", twin.Properties.Desired.Version);
            }
            return desiredPropertyValue;
        }

        public async Task<T?> ReadReportedComponentPropertyAsync<T>(string componentName, string propertyName)
        {
            var twin = await deviceClient.GetTwinAsync();
            var desiredPropertyValue = twin.Properties.Reported.GetPropertyValue<T>(componentName, propertyName);
            return desiredPropertyValue;
        }

        public async Task<T?> ReadReportedPropertyAsync<T>(string propertyName)
        {
            var twin = await deviceClient.GetTwinAsync();
            var desiredPropertyValue = twin.Properties.Reported.GetPropertyValue<T>(propertyName);
            return desiredPropertyValue;
        }

        private Task DesiredPropertyUpdateCallback(TwinCollection desiredProperties, object userContext)
        {
            var comps = desiredProperties.EnumerateComponents();
            foreach (var c in comps)
            {
                var ccb = desiredPropertyCallbacks[c];
                ccb?.Invoke(desiredProperties);
            }

            return Task.FromResult(0);
        }

        public async Task AckDesiredPropertyReadAsync(string componentName, string propertyName, object payload, StatusCodes statuscode, string description, long version)
        {
            var ack = CreateAck(componentName, propertyName, payload, statuscode, version, description);
            await deviceClient.UpdateReportedPropertiesAsync(ack);
        }

        private TwinCollection CreateAck(string componentName, string propertyName, object value, StatusCodes statusCode, long statusVersion, string statusDescription = "")
        {
            TwinCollection ack = new TwinCollection();
            var ackProps = new TwinCollection();
            ackProps["value"] = value;
            ackProps["ac"] = statusCode;
            ackProps["av"] = statusVersion;
            if (!string.IsNullOrEmpty(statusDescription)) ackProps["ad"] = statusDescription;
            TwinCollection ackChildren = new TwinCollection();
            ackChildren["__t"] = "c"; // TODO: Review, should the ACK require the flag
            ackChildren[propertyName] = ackProps;
            ack[componentName] = ackChildren;
            return ack;
        }

        public async ValueTask DisposeAsync()
        {
            await deviceClient.CloseAsync();
            deviceClient.Dispose();
        }
    }
}
