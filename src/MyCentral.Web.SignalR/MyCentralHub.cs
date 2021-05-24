using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace MyCentral.Web.Hubs
{
    public class MyCentralHub : Hub
    {
        private readonly EventHubConnections _connections;

        public MyCentralHub(EventHubConnections connections)
        {
            _connections = connections;
        }

        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();

            if (context.Request.Query.TryGetValue("host", out var hostname)
                && context.Request.Query.TryGetValue("eventsConnectionString", out var eventsConnectionString))
            {
                _connections.AddConnection(Context.ConnectionId, hostname, eventsConnectionString);
            }
            else
            {
                await Clients.Clients(Context.ConnectionId).SendAsync("UnknownHostName");
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _connections.RemoveConnection(Context.ConnectionId);

            return Task.CompletedTask;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
