using MyCentral.Device.Emulation;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client
{
    public class ClientEmulatedDeviceManager : IEmulatedDeviceManager
    {
        private readonly HttpClient _http;

        public ClientEmulatedDeviceManager(HttpClient http)
        {
            _http = http;
        }

        public DeviceCollection Collection => throw new NotImplementedException();

        public Task<IEmulatedDevice?> GetDeviceAsync(string id, CancellationToken token)
            => Task.FromResult<IEmulatedDevice?>(new EmulatedDevice(_http, id));

        public Task StartAsync(CancellationToken token)
            => Task.CompletedTask;

        private class EmulatedDevice : IEmulatedDevice
        {
            private readonly HttpClient _client;
            private readonly string _name;

            public EmulatedDevice(HttpClient client, string name)
            {
                _client = client;
                _name = name;
            }

            public ValueTask DisposeAsync() => default;

            public Task SendAsync(string componentName, byte[] bytes) => SendAsync(componentName, new ByteArrayContent(bytes));

            public Task SendAsync(string componentName, string content) => SendAsync(componentName, new StringContent(content));

            public Task SendAsync(string componentName, HttpContent content) => _client.PostAsync($"api/devices/{_name}/{componentName}", content);
        }
    }
}
