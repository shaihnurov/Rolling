using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerSignal.Models;
using ServerSignal.Service;

namespace ServerSignal.Hub
{
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly ApplicationContextDb _db;

        public ChatHub(ApplicationContextDb db)
        {
            _db = db;
        }
        
        public async Task ConnectChat()
        {
            UserData userData = await UserDataStorage.GetUserData();

            var user = _db.UserModels.Where(x => x.Email == userData.Email).Select(x => new {x.Name}).FirstOrDefault();
            await Clients.Caller.SendAsync("InitialChat", user);
        }
        
        public async Task Send(string username, string message)
        {
            await Clients.Caller.SendAsync("ReceiveChat", username, message);
        }
    }
}
