using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client.SignalR
{
    public class SignalrServiceClient : IServiceClient, IAsyncDisposable, INotifyPropertyChanged
    {
        private readonly HttpClient _client;
        private readonly CancellationTokenSource _cts;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SignalrServiceClient(IEventClient eventClient, HttpClient client)
        {
            Events = eventClient;
            _client = client;
            _cts = new CancellationTokenSource();

            _ = SetNameAsync();
        }

        public IEventClient Events { get; }

        public string HostName { get; private set; } = string.Empty;

        public ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _cts.Dispose();
            return Events.DisposeAsync();
        }

        public async IAsyncEnumerable<string> GetDevicesAsync([EnumeratorCancellation] CancellationToken token)
        {
            var collection = await _client.GetFromJsonAsync<IEnumerable<string>>("/api/devices", token);

            if (collection is not null)
            {
                foreach (var item in collection)
                {
                    yield return item;
                }
            }
        }

        private async Task SetNameAsync()
        {
            try
            {
                var metadata = await _client.GetFromJsonAsync<ServiceMetadata>("/api/devices/metadata", _cts.Token);

                if (metadata is not null)
                {
                    HostName = metadata.Name;
                    PropertyChanged?.Invoke(this, new(nameof(HostName)));
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task<string> InvokeMethodAsync(string deviceId, string methodName, string? payload = null)
        {
            using var response = await _client.PostAsync($"/api/devices/{deviceId}/invoke/{methodName}", new StringContent(payload ?? string.Empty));

            response.ThrowIfFailed();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdatePropertyAsync(string deviceId, string componentName, string propertyName, string propertyValue)
        {
            using var response = await _client.PostAsync($"/api/devices/{deviceId}/{componentName}/{propertyName}/{propertyValue}", new StringContent("{}"));

            response.ThrowIfFailed();
        }
    }
}
