using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MyCentral.Web.Hubs;
using System;

namespace MyCentral.Web
{
    public static class MyCentralSignalrServiceExtensions
    {
        public static void AddMyCentralSignalRService(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<EventHubConnections>();
            services.AddTransient<IObservable<EventNotification>>(ctx => ctx.GetRequiredService<EventHubConnections>());
            services.AddHostedService<EventHubBackgroundService>();
        }

        public static void MapMyCentralSignalr(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<MyCentralHub>("/events");
        }
    }
}
