using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace MyCentral.Client.SignalR
{
    public class SignalrEventClient : IEventClient
    {
        private readonly HubConnection _hubConnection;
        private readonly Subject<Event> _subject;
        private readonly Task _started;

        public SignalrEventClient(IOptions<EventClientOptions> options, ILoggerProvider loggingProvider)
        {
            var url = new UriBuilder(options.Value.ServiceEndpoint)
            {
                Path = "events",
            }.Uri;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .ConfigureLogging(logging =>
                {
                    logging.AddProvider(loggingProvider);
                })
                .WithAutomaticReconnect()
                .Build();

            _subject = new Subject<Event>();

            _hubConnection.On<Event>("ReceiveMessage", @event =>
            {
                _subject.OnNext(@event);
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
            await _hubConnection.DisposeAsync();

            _started.Dispose();
            _subject.OnCompleted();
            _subject.Dispose();
        }

        public IDisposable Subscribe(IObserver<Event> observer)
            => _subject.Subscribe(observer);
    }
}
