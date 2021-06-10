using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client
{
    public interface IServiceClient : IAsyncDisposable
    {
        string HostName { get; }

        IEventClient Events { get; }

        Task<DeviceCollection> GetDevicesAsync(CancellationToken token);
    }
}