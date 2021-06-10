using MyCentral.Client;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public interface IEmulatedDeviceFactory
    {
        Task<IEmulatedDevice> CreateDeviceAsync(DeviceInstance device, CancellationToken token);
    }
}