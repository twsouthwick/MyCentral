using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace IotSample.IotHub
{
    public static class ServiceExtensions
    {
        public static void AddIotHub(this IServiceCollection services)
        {
            services.AddSingleton<ServiceClientFactory>();
            services.AddMvcCore()
                .AddMvcOptions(options =>
                {
                    options.Filters.Add(new IotHubFilter());
                });
        }
    }

    public class IotHubFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (context.Exception is IotHubCommunicationException iotHubException)
            {
            }
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
        }
    }

    public class ServiceClientFactory
    {
        private readonly TokenCredential _credential;

        public ServiceClientFactory(TokenCredential credential)
        {
            _credential = credential;
        }

        public DeviceManager CreateClient(string name)
            => new DeviceManager(
                ServiceClient.Create(name, _credential),
                RegistryManager.Create(name, _credential));
    }

    public class DeviceManager : IAsyncDisposable
    {
        private readonly ServiceClient _client;
        private readonly RegistryManager _registry;

        public DeviceManager(ServiceClient client, RegistryManager registry)
        {
            _client = client;
            _registry = registry;
        }

        public ValueTask DisposeAsync()
        {
            _client.Dispose();
            _registry.Dispose();

            return new ValueTask();
        }

        public async IAsyncEnumerable<Twin> GetDevicesAsync([EnumeratorCancellation] CancellationToken token)
        {
            var query = _registry.CreateQuery(@"select deviceId,
                              lastActivityTime,
                              connectionState,
                              status,
                              properties.reported.[[$iotin:deviceinfo]].manufacturer.value as manufacturer
                       from devices
                       where capabilities.iotEdge != true");

            while (query.HasMoreResults)
            {
                var result = await query.GetNextAsJsonAsync();

                foreach (var r in result)
                {
                    yield break;
                }
            }
        }
    }
}
