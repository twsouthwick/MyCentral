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
        private readonly Dictionary<string, string> _connectionHostnameMapping = new();
        private readonly Dictionary<string, (IServiceClient, IDisposable)> _events = new();

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
                        await AddConnectionAsync(connection.ConnectionId, connection.Host, connection.EventConnectionString, stoppingToken);
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

        private async Task AddConnectionAsync(string connectionId, string hostname, string eventConnectionString, CancellationToken token)
        {
            var normalized = hostname.ToLowerInvariant();

            _logger.LogInformation("[{host}] Adding connection {ConnectionId}", normalized, connectionId);
            _connectionHostnameMapping.Add(connectionId, normalized);
            await _hubContext.Groups.AddToGroupAsync(connectionId, normalized, token);

            if (!_events.ContainsKey(normalized))
            {
                var serviceClient = _serviceClientFactory.CreateClient(hostname, eventConnectionString);
                var disposable = serviceClient.Events.Subscribe(new HubObserver(normalized, _hubContext, _logger));

                _events.Add(normalized, (serviceClient, disposable));
            }
        }

        private async Task RemoveConnectionAsync(string connectionId, CancellationToken token)
        {
            if (_connectionHostnameMapping.TryGetValue(connectionId, out var host))
            {
                _logger.LogInformation("[{host}] Adding connection {ConnectionId}", host, connectionId);
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, host, token);
            }
        }

        private class HubObserver : IObserver<Event>
        {
            private readonly string _name;
            private readonly IHubContext<MyCentralHub> _hubContext;
            private readonly ILogger _logger;

            public HubObserver(string name, IHubContext<MyCentralHub> hubContext, ILogger logger)
            {
                _name = name;
                _hubContext = hubContext;
                _logger = logger;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public async void OnNext(Event value)
            {
                _logger.LogInformation("[{host}] Received data", _name);
                await _hubContext.Clients.Group(_name).SendAsync("ReceiveMessage", value);
            }
        }
    }
}
