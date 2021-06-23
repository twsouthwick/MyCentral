using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    internal class EmulatedDeviceBackgroundService : BackgroundService
    {
        private readonly IEmulatedDeviceManager _manager;

        public EmulatedDeviceBackgroundService(IEmulatedDeviceManager manager)
        {
            _manager = manager;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _manager.StartAsync(stoppingToken);
        }
    }
}
