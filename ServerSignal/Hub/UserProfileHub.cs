using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerSignal.Models;
using ServerSignal.Service;

namespace ServerSignal.Hub;

public class UserProfileHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDb _db;

    public UserProfileHub(ApplicationContextDb db)
    {
        _db = db;
    }
    
    public async Task LoadUserData()
    {
        UserData userData = await UserDataStorage.GetUserData();

        var currentUser = _db.UserModels.Where(x => x.Email == userData.Email).Select(user => new { user.Id, user.Name, user.Email, user.Age, user.Level, user.Balance}).FirstOrDefault();
        
        await Clients.Caller.SendAsync("ReturnCurrentUser", currentUser, userData.Location);
    }
    public async Task ExitAccount()
    {
        UserDataStorage.DeleteUserData();
        
        await Clients.Caller.SendAsync("ExitAccount");
    }
}