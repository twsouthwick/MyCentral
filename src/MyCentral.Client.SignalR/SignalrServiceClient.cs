using System;
using System.Net;
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

        public async Task<string> InvokeMethodAsync(string deviceId, string methodName, string? payload = null)
        {
            using var response = await _client.PostAsync($"/api/devices/{deviceId}/invoke/{methodName}", new StringContent(payload ?? string.Empty));

            if (!response.IsSuccessStatusCode)
            {
                throw response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => throw new UnauthorizedAccessException(),
                    var code => throw new InvalidOperationException(code.ToString()),
                };
            }
            return await response.Content.ReadAsStringAsync();
        }
    }
}
