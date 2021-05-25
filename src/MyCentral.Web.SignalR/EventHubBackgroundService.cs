using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using MyCentral.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MyCentral.Web.Hubs
{
    public class EventHubBackgroundService : BackgroundService
    {
        private readonly Dictionary<string, HubObserver> _connectionHostnameMapping = new();

        private readonly IServiceClientFactory _serviceClientFactory;
        private readonly EventHubConnections EventHubConnections;
        private readonly IHubContext<MyCentralHub> _hubContext;
        private readonly ILogger<EventHubBackgroundService> _logger;

        public EventHubBackgroundService(
            IServiceClientFactory serviceClientFactory,
            EventHubConnections events,
            IHubContext<MyCentralHub> hubContext,
            ILogger<EventHubBackgroundService> logger)
        {
            _serviceClientFactory = serviceClientFactory;
            EventHubConnections = events;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var connection in EventHubConnections.ToAsyncEnumerable().WithCancellation(stoppingToken))
            {
                _logger.LogInformation("Dispatching new connection change");
                try
                {
                    if (connection.State == EventState.Connected)
                    {
                        if (connection.Host is not null && connection.EventConnectionString is not null)
                        {
                            await AddConnectionAsync(connection.ConnectionId, connection.Host, connection.EventConnectionString, stoppingToken);
                        }
                        else
                        {
                            _logger.LogWarning("Host or event connection string was null");
                        }
                    }
                    else if (connection.State == EventState.Disconnected)
                    {
                        await RemoveConnectionAsync(connection.ConnectionId, stoppingToken);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error processing connections");
                }
            }
        }

        private Task AddConnectionAsync(string connectionId, string hostname, string eventConnectionString, CancellationToken token)
        {
            var normalized = hostname.ToLowerInvariant();

            _logger.LogInformation("[{host}] Adding connection {ConnectionId}", normalized, connectionId);
            var serviceClient = _serviceClientFactory.CreateClient(hostname, eventConnectionString);
            var connection = new HubObserver(connectionId, serviceClient, _hubContext, _logger);
            _connectionHostnameMapping.Add(connectionId, connection);

            return Task.CompletedTask;
        }

        private async Task RemoveConnectionAsync(string connectionId, CancellationToken token)
        {
            if (_connectionHostnameMapping.TryGetValue(connectionId, out var host))
            {
                _logger.LogInformation("[{host}] Removing connection {ConnectionId}", host.HostName, connectionId);
                await host.DisposeAsync();
            }
        }

        private sealed record ConnectionData(IServiceClient Client, IDisposable Subscription) : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                Subscription.Dispose();
                await Client.DisposeAsync();
            }
        }

        private class HubObserver : IObserver<Event>, IAsyncDisposable
        {
            private readonly string _connectionId;
            private readonly IServiceClient _client;
            private readonly IDisposable _subscription;
            private readonly IHubContext<MyCentralHub> _hubContext;
            private readonly ILogger _logger;

            public HubObserver(
                string connectionId,
                IServiceClient client,
                IHubContext<MyCentralHub> hubContext,
                ILogger logger)
            {
                _connectionId = connectionId;
                _client = client;
                _subscription = _client.Events.Subscribe(this);
                _hubContext = hubContext;
                _logger = logger;
            }

            public string HostName => _client.HostName;

            public ValueTask DisposeAsync()
            {
                _subscription.Dispose();
                return _client.DisposeAsync();
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public async void OnNext(Event value)
            {
                _logger.LogInformation("[{host}] Received data", _client.HostName);
                await _hubContext.Clients.Client(_connectionId).SendAsync("ReceiveMessage", value);
            }
        }
    }
}
