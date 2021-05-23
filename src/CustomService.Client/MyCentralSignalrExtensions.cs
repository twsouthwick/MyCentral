using Microsoft.Extensions.DependencyInjection;
using MyCentral.Client.SignalR;
using System;

namespace MyCentral.Client
{
    public static class MyCentralSignalRExtensions
    {
        public static void AddSignalRMyCentral(this IServiceCollection services, Action<EventClientOptions> configure)
        {
            services.AddSingleton<SignalrEventClientFactory>();
            services.AddSingleton<ServiceClientFactory>();
            services.AddOptions<EventClientOptions>()
                .Configure(configure);
        }
    }
}
