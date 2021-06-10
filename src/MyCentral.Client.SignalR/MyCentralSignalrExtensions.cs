using Microsoft.Extensions.DependencyInjection;
using MyCentral.Client.SignalR;
using MyCentral.Device.Emulation;
using System;

namespace MyCentral.Client
{
    public static class MyCentralSignalRExtensions
    {
        public static void AddSignalRMyCentral(this IServiceCollection services, Action<EventClientOptions> configure)
        {
            services.AddScoped<IEventClient, SignalrEventClient>();
            services.AddScoped<IServiceClient, SignalrServiceClient>();
            services.AddScoped<IEmulatedDeviceManager, ClientEmulatedDeviceManager>();
            services.AddOptions<EventClientOptions>()
                .Configure(configure);
        }
    }
}
