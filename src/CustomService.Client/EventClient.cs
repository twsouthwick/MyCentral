using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace CustomService.Client
{
    public class EventClient : IAsyncDisposable, IObservable<Item>
    {
        private readonly HubConnection hubConnection;
        private readonly Subject<Item> _subject;
        private readonly Task _started;

        public EventClient(IOptions<EventClientOptions> options)
        {
            var url = new Uri(new Uri(options.Value.ServiceEndpoint), "events");

            hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _subject = new Subject<Item>();

            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                _subject.OnNext(new Item(user, message));
            });

            _started = hubConnection.StartAsync();
        }

        public bool IsConnected => hubConnection.State == HubConnectionState.Connected;

        public async Task Send(string userInput, string message)
        {
            await _started;
            await hubConnection.SendAsync("SendMessage", userInput, message);
        }

        public async ValueTask DisposeAsync()
        {
            _started.Dispose();
            _subject.OnCompleted();
            _subject.Dispose();
            await hubConnection.DisposeAsync();
        }

        public IDisposable Subscribe(IObserver<Item> observer)
            => _subject.Subscribe(observer);
    }
}
