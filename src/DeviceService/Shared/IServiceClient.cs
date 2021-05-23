using System;

namespace MyCentral.Client
{
    public interface IServiceClient : IAsyncDisposable
    {
        string HostName { get; }

        IObservable<Item> Events { get; }
    }
}