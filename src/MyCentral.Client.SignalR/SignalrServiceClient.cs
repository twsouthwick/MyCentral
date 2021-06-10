using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client.SignalR
{
    public class SignalrServiceClient : IServiceClient, IAsyncDisposable
    {
        private readonly HttpClient _client;

        public SignalrServiceClient(IEventClient eventClient, HttpClient client)
        {
            Events = eventClient;
            _client = client;
        }

        public IEventClient Events { get; }

        public string HostName { get; } = string.Empty;

        public ValueTask DisposeAsync()
        {
            return Events.DisposeAsync();
        }

        public async Task<DeviceCollection> GetDevicesAsync(CancellationToken token)
        {
            var collection = await _client.GetFromJsonAsync<DeviceCollection>("/api/devices", token);

            return collection ?? new DeviceCollection();
        }
    }
}
