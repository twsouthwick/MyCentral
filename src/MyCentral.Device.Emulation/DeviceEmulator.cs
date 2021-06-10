using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyCentral.Device.Emulation
{
    public class DeviceEmulator : BackgroundService
    {
        public readonly DeviceManager _manager;
        private readonly IEmulatedDeviceFactory _factory;
        private readonly ILogger<DeviceEmulator> _logger;

        public DeviceEmulator(
            DeviceManager manager,
            IEmulatedDeviceFactory factory,
            ILogger<DeviceEmulator> logger)
        {
            _manager = manager;
            _factory = factory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var devices = await Task.WhenAll(_manager.Collection.Devices.Select(d => _factory.CreateDeviceAsync(d, stoppingToken)));

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Emulating devices.....");
                    await Task.Delay(TimeSpan.FromSeconds(100), stoppingToken);
                }
            }
            finally
            {
                foreach (var device in devices)
                {
                    await device.DisposeAsync();
                }
            }
        }
    }
}
