using System;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public interface IEmulatedDevice : IAsyncDisposable
    {
        Task SendAsync<T>(string componentName, T value);
    }
}