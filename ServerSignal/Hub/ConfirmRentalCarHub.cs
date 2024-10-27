using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using ServerSignal.Models;
using ServerSignal.Service;

namespace ServerSignal.Hub;

public class ConfirmRentalCarHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDb _db;

    public ConfirmRentalCarHub(ApplicationContextDb db)
    {
        _db = db;
    }
    
    public async Task PaymentRental(double totalPrice, Guid carId, string rentalDays)
    {
        UserData currentUserData = await UserDataStorage.GetUserData();

        if (string.IsNullOrEmpty(currentUserData.Email))
        {
            await Clients.Caller.SendAsync("ErrorPayment", "Unable to find the user");
            return;
        }

        var currentUser = await _db.UserModels.FirstOrDefaultAsync(x => x.Email == currentUserData.Email);

        if (currentUser == null)
        {
            await Clients.Caller.SendAsync("ErrorPayment", "Unable to find the user");
            return;
        }

        if (currentUser.Balance < totalPrice)
        {
            await Clients.Caller.SendAsync("ErrorPayment", "Insufficient funds on the balance sheet");
            return;
        }
        
        var currentCar = await _db.CarsRentalModels.FirstOrDefaultAsync(x => x.Id == carId);
        
        if (currentCar != null)
        {
            var sql = @"UPDATE CarsRentalModels SET Status = @Status WHERE Id = @Id";

            await _db.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@Status", true),
                new SqlParameter("@Id", currentCar.Id)
            );
            
            var newRental = new ListRentalsModel
            {
                CarId = currentCar.Id,
                UserId = currentUser.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(int.Parse(rentalDays)) 
            };
        
            await _db.ListRentalsModels.AddAsync(newRental);
            currentUser.Balance -= totalPrice;

            await _db.SaveChangesAsync();

            await Clients.Caller.SendAsync("SuccessfulPaymentRental");
        }
        else
        {
            await Clients.Caller.SendAsync("ErrorPayment", "Could not find the selected vehicle");
        }
    }
}
