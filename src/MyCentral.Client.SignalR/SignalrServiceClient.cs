using System;
using System.Threading.Tasks;

namespace MyCentral.Client.SignalR
{
    public class SignalrServiceClient : IServiceClient, IAsyncDisposable
    {
        public SignalrServiceClient(IEventClient eventClient)
        {
            Events = eventClient;
        }

        public IEventClient Events { get; }

        public string HostName { get; } = string.Empty;

        public ValueTask DisposeAsync()
        {
            return Events.DisposeAsync();
        }
    }
}
