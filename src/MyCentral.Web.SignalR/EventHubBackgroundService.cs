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

        private readonly IServiceClient _serviceClient;
        private readonly EventHubConnections EventHubConnections;
        private readonly IHubContext<MyCentralHub> _hubContext;
        private readonly ILogger<EventHubBackgroundService> _logger;

        public EventHubBackgroundService(
            IServiceClient serviceClient,
            EventHubConnections events,
            IHubContext<MyCentralHub> hubContext,
            ILogger<EventHubBackgroundService> logger)
        {
            _serviceClient = serviceClient;
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
                        AddConnection(connection.ConnectionId);
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

        private void AddConnection(string connectionId)
        {
            _logger.LogInformation("Adding connection {ConnectionId}", connectionId);
            var connection = new HubObserver(connectionId, _serviceClient, _hubContext, _logger);
            _connectionHostnameMapping.Add(connectionId, connection);
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
                _logger.LogTrace("[{host}] Received data", _client.HostName);
                await _hubContext.Clients.Client(_connectionId).SendAsync("ReceiveMessage", value);
            }
        }
    }
}
