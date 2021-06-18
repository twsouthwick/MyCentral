using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public interface IEmulatedDevice : IAsyncDisposable
    {
        Task SendAsync(string componentName, byte[] content);

        public Task SendAsync(string componentName, string content)
            => SendAsync(componentName, Encoding.UTF8.GetBytes(content));

        public async Task SendAsync(string componentName, Stream content)
        {
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            await SendAsync(componentName, ms.ToArray());
        }
    }
}