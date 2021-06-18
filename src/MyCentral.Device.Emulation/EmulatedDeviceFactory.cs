using Microsoft.Azure.Devices.Client;
using MyCentral.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public class EmulatedDeviceFactory : IEmulatedDeviceFactory
    {
        private readonly IServiceClient _client;

        public EmulatedDeviceFactory(IServiceClient client)
        {
            _client = client;
        }

        public async Task<IEmulatedDevice> CreateDeviceAsync(DeviceInstance device, CancellationToken token)
        {
            var dc = DeviceClient.CreateFromConnectionString(device.GetConnectionString(_client.HostName), TransportType.Mqtt, new ClientOptions { ModelId = device.ModelId });
            var client = new PnPClient(dc);

            await client.ReportComponentPropertyAsync("deviceInfo", "manufacturer", "mycentral");

            client.SetDesiredPropertyUpdateCommandHandler("thermostat1", async (twin) =>
            {
                var targetTemp1 = twin.GetPropertyValue<int>("thermostat1", "targetTemperature");
                await client.AckDesiredPropertyReadAsync("thermostat1", "targetTemperature", targetTemp1, StatusCodes.Completed, "tt1 received", twin.Version);
            });

            client.SetDesiredPropertyUpdateCommandHandler("thermostat2", async (twin) =>
            {
                var targetTemp2 = twin.GetPropertyValue<int>("thermostat2", "targetTemperature");
                await client.AckDesiredPropertyReadAsync("thermostat2", "targetTemperature", targetTemp2, StatusCodes.Completed, "tt2 received", twin.Version);
            });

            await client.SetCommandHandlerAsync("reboot", async (req, ctx) =>
            {
                await Task.Delay(1);
                return new MethodResponse(Encoding.UTF8.GetBytes("{}"), 200);
            }, null);

            await client.SetComponentCommandHandlerAsync("thermostat1", "getMaxMinReport", async (req, ctx) =>
            {
                await Task.Delay(1);
                return new MethodResponse(Encoding.UTF8.GetBytes("{}"), 200);
            }, null);

            await client.SetComponentCommandHandlerAsync("thermostat2", "getMaxMinReport", async (req, ctx) =>
            {
                await Task.Delay(1);
                return new MethodResponse(Encoding.UTF8.GetBytes("{}"), 200);
            }, null);

            return client;
        }
    }
}
