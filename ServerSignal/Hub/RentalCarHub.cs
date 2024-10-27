using System.Collections.ObjectModel;
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
    public async Task ListUserRental()
    {
        UserData userData = await UserDataStorage.GetUserData();

        var currentUser = await _db.UserModels.FirstOrDefaultAsync(user => user.Email == userData.Email);

        if (currentUser == null)
        {
            await Clients.Caller.SendAsync("ErrorListUserRental", "User not found");
            return;
        }
        
        var rentalList = await _db.ListRentalsModels.Where(x => x.UserId == currentUser.Id).ToListAsync();

        var rentalCarIds = rentalList.Select(rental => rental.CarId).ToList();
        
        var cars = await _db.CarsRentalModels.Where(car => rentalCarIds.Contains(car.Id)).ToListAsync();

        var infoRentalCars = new ObservableCollection<RentalCarInfoDto>();

        foreach (var rental in rentalList)
        {
            var car = cars.FirstOrDefault(c => c.Id == rental.CarId);
            if (car != null)
            {
                infoRentalCars.Add(new RentalCarInfoDto
                {
                    Id = car.Id,
                    Mark = car.Mark,
                    Model = car.Model,
                    Years = car.Years,
                    Color = car.Color,
                    HorsePower = car.HorsePower,
                    Mileage = car.Mileage,
                    Engine = car.Engine,
                    Price = car.Price,
                    City = car.City,
                    Status = car.Status,
                    Image = car.Image,
                    EndDate = rental.EndDate
                });
            }
        }

        await Clients.Caller.SendAsync("RentalListUser", infoRentalCars);
    }
}