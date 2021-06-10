using System;
using System.IO;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public interface IEmulatedDevice : IAsyncDisposable
    {
        Task SendAsync(string componentName, Stream content);

        Task SendAsync(string componentName, string content);
    }
}