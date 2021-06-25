using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client
{
    public interface IServiceClient : IAsyncDisposable
    {
        string HostName { get; }

        IEventClient Events { get; }

        IAsyncEnumerable<string> GetDevicesAsync(CancellationToken token);

        Task<string> InvokeMethodAsync(string deviceId, string methodName, string? payload = null);
    }
}