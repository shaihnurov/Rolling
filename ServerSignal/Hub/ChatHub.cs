using Microsoft.AspNetCore.SignalR;

namespace ServerSignal.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task Send(string username, string message)
        {
            await Clients.All.SendAsync("ReceiveChat", username, message);
        }
    }
}
