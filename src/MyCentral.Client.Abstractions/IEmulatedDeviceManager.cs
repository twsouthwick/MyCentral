using MyCentral.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public interface IEmulatedDeviceManager
    {
        DeviceCollection Collection { get; }

        Task<IEmulatedDevice?> GetDeviceAsync(string id, CancellationToken token);

        Task StartAsync(CancellationToken token);
    }
}