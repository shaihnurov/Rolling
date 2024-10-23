using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerSignal.Models;
using ServerSignal.Service;
using ServerSignal.Models;
using ServerSignal.Service;

namespace ServerSignal.Hub;

public class AuthHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDb _db;
    private readonly GeoLocationService _geoLocationService;

    public AuthHub(ApplicationContextDb db)
    {
        _db = db;
        _geoLocationService = new GeoLocationService();
    }

    public async Task AuthUser(string email, string password, bool saveData)
    {
        var currentUser = await _db.UserModels.FirstOrDefaultAsync(s => s.Email == email);
        string location = await GetLocation();

        if (currentUser == null)
        {
            await Clients.Caller.SendAsync("AuthErrorNotFound", "User not found");
        }
        else
        {
            bool _checkPass = BCrypt.Net.BCrypt.Verify(password, currentUser.Password);

            if (_checkPass)
            {
                if (saveData)
                {
                    string token = TokenService.GenerateToken(email);
                    var userData = new UserData
                    {
                        Token = token,
                        Email = email,
                        Location = location,
                        ExpiryDate = DateTime.UtcNow.AddHours(7)
                    };
                    await UserDataStorage.SaveUserData(userData);
                }
                else
                {
                    var userData = new UserData
                    {
                        Email = email,
                        Location = location
                    };
                    await UserDataStorage.SaveUserData(userData);
                }
                bool permission = await CheckUserPermission(email);
                await Clients.Caller.SendAsync("AuthUser", permission);
            }else
                await Clients.Caller.SendAsync("AuthUserInvalidPass", "Invalid password");
        }
    }
    public async Task AuthToken()
    {
        UserData storedUserData = await UserDataStorage.GetUserData();

        if (storedUserData != null && !string.IsNullOrEmpty(storedUserData.Token))
        {
            var claimsPrincipal = TokenService.ValidateToken(storedUserData.Token);
            if (claimsPrincipal != null)
            {
                bool permission = await CheckUserPermission(storedUserData.Email);
                
                await Clients.Caller.SendAsync("AuthToken", permission);
            }
            else
            {
                UserDataStorage.DeleteUserData();
                await Clients.Caller.SendAsync("AuthInvalidToken", "Invalid session");
            }
        }
        else
        {
            Console.WriteLine("user token not found");
            await Clients.Caller.SendAsync("AuthTokenNotFound", "Invalid token");
        }
    }
    private async Task<bool> CheckUserPermission(string email)
    {
        var user = await _db.UserModels.FirstOrDefaultAsync(s => s.Email == email);

        if (user != null && user.Permission == "User")
            return true;
        return false;
    }
    private async Task<string> GetLocation()
    {
        return await _geoLocationService.GetLocation();
    }
}