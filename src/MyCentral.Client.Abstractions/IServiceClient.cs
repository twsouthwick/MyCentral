using System;

namespace MyCentral.Client
{
    public interface IServiceClient : IAsyncDisposable
    {
        string HostName { get; }

        IEventClient Events { get; }
    }
}