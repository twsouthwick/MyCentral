using System;
using System.Threading.Tasks;

namespace MyCentral.Client.SignalR
{
    public class SignalrServiceClient : IServiceClient, IAsyncDisposable
    {
        public SignalrServiceClient(string hostname, IEventClient eventClient)
        {
            HostName = hostname;
            Events = eventClient;
        }

        public IEventClient Events { get; }

        public string HostName { get; }

        public ValueTask DisposeAsync()
        {
            return Events.DisposeAsync();
        }
    }
}
