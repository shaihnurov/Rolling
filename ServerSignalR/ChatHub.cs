using Microsoft.AspNetCore.SignalR;

namespace ServerSignalR;

public class ChatHub : Hub
{
    public async Task Send(string userName, string message)
    {
        await this.Clients.All.SendAsync("Receive", userName, message);
    }
}