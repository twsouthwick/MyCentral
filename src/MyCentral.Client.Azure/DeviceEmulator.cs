using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Client.Azure
{
    public class DeviceEmulator : BackgroundService
    {
        public readonly DeviceManager _manager;

        public DeviceEmulator(DeviceManager manager)
        {
            _manager = manager;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
