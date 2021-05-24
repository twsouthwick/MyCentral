using System;

namespace MyCentral.Client
{
    public interface IEventClient : IObservable<Item>, IAsyncDisposable
    {
    }
}
