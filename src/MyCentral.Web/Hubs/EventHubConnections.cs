using Microsoft.AspNetCore.Builder;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCentral.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Reactive.Subjects;

namespace MyCentral.Web.Hubs
{
    public static class MyCentralSignalrServiceExtensions
    {
        public static void AddMyCentralSignalRService(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<EventHubConnections>();
            services.AddTransient<IObservable<EventNotification>>(ctx => ctx.GetRequiredService<EventHubConnections>());
            services.AddHostedService<EventHubBackgroundService>();
        }

        public static void MapMyCentralSignalr(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<IoTEventHub>("/events");
        }
    }
    public class EventHubBackgroundService : BackgroundService, IObserver<Item>
    {
        private readonly Dictionary<string, string> _connectionHostnameMapping = new();
        private readonly Dictionary<string, (IServiceClient, IDisposable)> _events = new();

        private readonly IServiceClientFactory _serviceClientFactory;
        private readonly EventHubConnections EventHubConnections;
        private readonly IHubContext<IoTEventHub> _hubContext;
        private readonly ILogger<EventHubBackgroundService> _logger;

        public EventHubBackgroundService(
            IServiceClientFactory serviceClientFactory,
            EventHubConnections events,
            IHubContext<IoTEventHub> hubContext,
            ILogger<EventHubBackgroundService> logger)
        {
            _serviceClientFactory = serviceClientFactory;
            EventHubConnections = events;
            _hubContext = hubContext;
            _logger = logger;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public async void OnNext(Item value)
        {
            _logger.LogInformation("[{host}] Received data", "asdf");
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", value.User, value.Message);
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
                var disposable = serviceClient.Events.Subscribe(this);

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
    }

    public enum EventState { None, Connected, Disconnected };

    public record EventNotification(EventState State, string ConnectionId)
    {
        public string Host { get; init; }
        public string EventConnectionString { get; init; }
    }

    public class EventHubConnections : IObservable<EventNotification>, IDisposable
    {
        private readonly Subject<EventNotification> _subject = new();

        public void AddConnection(string id, string hostname, string eventConnectionString)
        {
            _subject.OnNext(new EventNotification(EventState.Connected, id)
            {
                Host = hostname,
                EventConnectionString = eventConnectionString
            });
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        public void RemoveConnection(string id)
        {
            _subject.OnNext(new EventNotification(EventState.Disconnected, id));
        }

        IDisposable IObservable<EventNotification>.Subscribe(IObserver<EventNotification> observer)
            => _subject.Subscribe(observer);
    }
}
