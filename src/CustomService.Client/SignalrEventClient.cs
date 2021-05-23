using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace MyCentral.Client.SignalR
{
    public class ServiceClientFactory
    {
        private readonly SignalrEventClientFactory _eventClientFactory;

        public ServiceClientFactory(SignalrEventClientFactory eventClientFactory)
        {
            _eventClientFactory = eventClientFactory;
        }

        public SignalrServiceClient CreateClient(string hostname)
            => new(hostname, _eventClientFactory.Create(hostname));
    }

    public class SignalrServiceClient : IServiceClient, IAsyncDisposable
    {
        public SignalrServiceClient(string hostname, SignalrEventClient eventClient)
        {
            HostName = hostname;
            EventClient = eventClient;
        }

        public SignalrEventClient EventClient { get; }

        public string HostName { get; }

        public IObservable<Item> Events => EventClient;

        public ValueTask DisposeAsync()
        {
            return EventClient.DisposeAsync();
        }
    }

    public class SignalrEventClient : IAsyncDisposable, IObservable<Item>
    {
        private readonly HubConnection _hubConnection;
        private readonly Subject<Item> _subject;
        private readonly Task _started;

        public SignalrEventClient(IOptions<EventClientOptions> options, ILoggerProvider loggingProvider, string hostname)
        {
            var url = new UriBuilder(options.Value.ServiceEndpoint)
            {
                Path = "events",
                Query = $"host={hostname}"
            }.Uri;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .ConfigureLogging(logging =>
                {
                    logging.AddProvider(loggingProvider);
                })
                .WithAutomaticReconnect()
                .Build();

            _subject = new Subject<Item>();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                _subject.OnNext(new Item(user, message));
            });

            _hubConnection.On("Unauthorized", () =>
            {
                _subject.OnError(new UnauthorizedAccessException());
            });

            _started = _hubConnection.StartAsync();
        }

        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

        public async Task Send(string userInput, string message)
        {
            await _started;
            await _hubConnection.SendAsync("SendMessage", userInput, message);
        }

        public async ValueTask DisposeAsync()
        {
            _started.Dispose();
            _subject.OnCompleted();
            _subject.Dispose();
            await _hubConnection.DisposeAsync();
        }

        public IDisposable Subscribe(IObserver<Item> observer)
            => _subject.Subscribe(observer);
    }
}
