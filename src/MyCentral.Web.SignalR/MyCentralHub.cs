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

        public override Task OnConnectedAsync()
        {
            _connections.AddConnection(Context.ConnectionId);

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
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
