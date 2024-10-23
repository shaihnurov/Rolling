using System.Net.Mail;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using ServerSignal.Models;
using ServerSignal.Service;
using Exception = System.Exception;

namespace ServerSignal.Hub;

public class RegisterHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDb _db;
    private readonly GeoLocationService _geoLocationService;

    public RegisterHub(ApplicationContextDb db)
    {
        _db = db;
        _geoLocationService = new GeoLocationService();
    }

    public async Task CheckUserEmail(string name, string email)
    {
        var existingUser = await _db.UserModels.FirstOrDefaultAsync(s => s.Email == email);

        try
        {
            if (existingUser != null)
            {
                await Clients.Caller.SendAsync("RegisterError", "This email is already registered");
            }
            else
            {
                string _verifycode = await SendVerificationCode.SendVerification(name, email);

                await Clients.Caller.SendAsync("CheckUserEmail", _verifycode);
            }
        }
        catch (SmtpException ex)
        {
            await Clients.Caller.SendAsync("RegisterError", ex.Message);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("RegisterError", ex.Message);
        }
    }
    public async Task RegisterUser(string name, int age, string email, string password, string code, string verifycode)
    {
        if (verifycode == code)
        {
            string location = await GetLocation();

            var userModel = new UserModel
            {
                Name = name,
                Age = age,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Level = 1,
                Permission = "User"
            };

            var userData = new UserData
            {
                Email = email,
                Location = location,
            };
            await UserDataStorage.SaveUserData(userData);
            await _db.UserModels.AddAsync(userModel);
            await _db.SaveChangesAsync();

            await Clients.Caller.SendAsync("RegisterUser");
        }
        else
        {
            await Clients.Caller.SendAsync("RegisterError", "Incorrect confirmation code");
        }
    }
    private async Task<string> GetLocation()
    {
        return await _geoLocationService.GetLocation();
    }
}