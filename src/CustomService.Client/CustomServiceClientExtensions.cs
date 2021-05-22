using Microsoft.Extensions.DependencyInjection;
using System;

namespace CustomService.Client
{
    public static class CustomServiceClientExtensions
    {
        public static void AddCustomServiceClient(this IServiceCollection services, Action<EventClientOptions> configure)
        {
            services.AddScoped<EventClient>();
            services.AddOptions<EventClientOptions>()
                .Configure(configure);
        }
    }
}
