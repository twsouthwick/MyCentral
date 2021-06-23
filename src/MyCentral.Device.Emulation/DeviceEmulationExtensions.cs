using Microsoft.Extensions.DependencyInjection;
using MyCentral.Client;
using System;

namespace MyCentral.Device.Emulation
{
    public static class DeviceEmulationExtensions
    {
        public static void AddDeviceEmulation(this IServiceCollection services, Action<DeviceCollection> configure)
        {
            services.AddSingleton<IEmulatedDeviceFactory, EmulatedDeviceFactory>();
            services.AddSingleton<IEmulatedDeviceManager, DeviceManager>();
            services.AddOptions<DeviceCollection>()
                .Configure(configure);
            services.AddHostedService<EmulatedDeviceBackgroundService>();
        }
    }
}
