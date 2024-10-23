using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using ServerSignal.Models;
using ServerSignal.Service;

namespace ServerSignal.Hub;

public class RentalCarHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDb _db;

    public RentalCarHub(ApplicationContextDb db)
    {
        _db = db;
    }

    public async Task GetCars()
    {
        UserData userData = await UserDataStorage.GetUserData();

        if(userData.Location != null)
        {
            var carsInLocation = await _db.CarsRentalModels.Where(car => car.City == userData.Location).ToListAsync();
            await Clients.All.SendAsync("RentalCarsList", carsInLocation);
        }
        else
        {
            await Clients.All.SendAsync("RentalCarsListInvalidLocation");
        }
    }
}