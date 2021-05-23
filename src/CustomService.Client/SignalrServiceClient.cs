using System;
using System.Threading.Tasks;

namespace MyCentral.Client.SignalR
{
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
}
