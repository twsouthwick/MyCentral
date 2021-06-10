using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MyCentral.Client.Azure
{
    public static class MyCentralAzureExtensions
    {
        public static void AddAzureMyCentral(this IServiceCollection services, Action<IoTHubOptions> configure)
        {
            services.AddSingleton<IServiceClient, AzureServiceClient>();
            services.AddOptions<IoTHubOptions>()
                .Configure(configure);
        }
    }
}
