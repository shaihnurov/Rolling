using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;

namespace Rolling.ViewModels
{
    public class AdminViewModel : ObservableObject
    {
        private ObservableCollection<CarsRentalModel> _carsRental;

        public ObservableCollection<CarsRentalModel> CarsRental
        {
            get => _carsRental;
            set => SetProperty(ref _carsRental, value);
        }

        public AsyncRelayCommand BtnSaveChangedCommand { get; set; }
        
        public AdminViewModel()
        {
            CarsRental = new ObservableCollection<CarsRentalModel>();
            LoadCollectionCars();
            BtnSaveChangedCommand = new AsyncRelayCommand(SaveChanges);
        }

        private void LoadCollectionCars()
        {
            Task.Run(async () =>
            {
                using (ApplicationContextDb db = new())
                {
                    var cars = await db.CarsRentalModels.ToListAsync();

                    foreach (var item in cars)
                    {
                        CarsRental.Add(item);
                    }
                }
            });
        }
        private async Task SaveChanges()
        {
            if (CarsRental.Count != 0)
            {
                using (ApplicationContextDb db = new())
                {
                    foreach (var carsModel in CarsRental)
                    {
                        var existingCar = await db.CarsRentalModels.FirstOrDefaultAsync(s => s.Id == carsModel.Id);

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
                            
                            db.Entry(existingCar).State = EntityState.Modified;
                        }
                    }

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}