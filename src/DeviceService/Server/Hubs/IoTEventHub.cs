using IotSample.IotHub;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceService.Server.Hubs
{
    public class IoTEventHub : Hub
    {
        private readonly EventHubConnections _connections;

        public IoTEventHub(EventHubConnections connections)
        {
            _connections = connections;
        }

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();

            if (context.Request.Query.TryGetValue("host", out var hostname))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, hostname);
                _connections.AddHostName(Context.ConnectionId, hostname);
            }
            else
            {
                await Clients.Clients(Context.ConnectionId).SendAsync("UnknownHostName");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryRemove(Context.ConnectionId, out var hostname))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, hostname);
            }
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
