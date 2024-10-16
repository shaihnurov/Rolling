using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;

namespace Server
{
    public class CarHub : Hub
    {
        private readonly ApplicationContextDb _db;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CarHub(ApplicationContextDb applicationContextDb, IConfiguration configuration)
        {
            _db = applicationContextDb;
            _configuration = configuration;
            
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
        }
        
        public async Task GetCars()
        {
            var cars = await _db.CarsRentalModels.ToListAsync();
            await Clients.Caller.SendAsync("ReceiveCars", cars);
        }
        public async Task SaveChangesCars(ObservableCollection<CarsRentalModel> carsRental)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
        
                var command = new SqlCommand("SELECT [Id] FROM CarsRentalModels", connection);
                var dependency = new SqlDependency(command);
                dependency.OnChange += new OnChangeEventHandler(OnDependencyChange);
                command.ExecuteReader();

                foreach (var carsModel in carsRental)
                {
                    var existingCar = await _db.CarsRentalModels.FirstOrDefaultAsync(s => s.Id == carsModel.Id);

                    if (existingCar != null)
                    {
                        existingCar.Mark = carsModel.Mark;
                        existingCar.Model = carsModel.Model;
                        existingCar.Year = carsModel.Year;
                        existingCar.Color = carsModel.Color;
                        existingCar.HorsePower = carsModel.HorsePower;
                        existingCar.Mileage = carsModel.Mileage;
                        existingCar.Engine = carsModel.Engine;
                        existingCar.Price = carsModel.Price;
                        existingCar.Status = carsModel.Status;

                        _db.Entry(existingCar).State = EntityState.Modified;
                    }
                }
                await _db.SaveChangesAsync();
            }
            await GetCars();
        }
        public void RegisterForNotifications()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT [Id], [Mark] FROM CarsRentalModels", connection);
                var dependency = new SqlDependency(command);
                dependency.OnChange += new OnChangeEventHandler(OnDependencyChange);
                command.ExecuteReader();
            }
        }
        private void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                var updatedCars = _db.CarsRentalModels.ToList();
                Clients.All.SendAsync("ReceiveCars", updatedCars);
            }
        }
    }
}