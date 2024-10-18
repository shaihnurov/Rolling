using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels
{
    public class AdminViewModel : ObservableObject, IServerConnectionHandler
    {
        private HubConnection _hubConnection;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private ObservableCollection<CarsRentalModel> _carsRental;

        public ObservableCollection<CarsRentalModel> CarsRental
        {
            get => _carsRental;
            set => SetProperty(ref _carsRental, value);
        }
        public AsyncRelayCommand BtnSaveChangedCommand { get; set; }

        public AdminViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            CarsRental = [];
            BtnSaveChangedCommand = new AsyncRelayCommand(SaveChanges);
        }

        private async Task SaveChanges()
        {
            
        }
        public async void ConnectToSignalR()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder().WithUrl("https://localhost:7160/carhub").Build();

                _hubConnection.On<CarsRentalModel>("ReceiveCarUpdate", (car) => {
                    Console.WriteLine($"Received update for car ID: {car.Id}");
                    Dispatcher.UIThread.Post(() =>
                    {
                        var existingCar = CarsRental.FirstOrDefault(c => c.Id == car.Id);
                        if (existingCar != null)
                        {
                            existingCar.Mark = car.Mark;
                            existingCar.Model = car.Model;
                            existingCar.Years = car.Years;
                            existingCar.Color = car.Color;
                            existingCar.HorsePower = car.HorsePower;
                            existingCar.Mileage = car.Mileage;
                            existingCar.Engine = car.Engine;
                            existingCar.Price = car.Price;
                            existingCar.City = car.City;
                            existingCar.Status = car.Status;
                        }
                        else
                        {
                            CarsRental.Add(car);
                        }
                    });
                });
                _hubConnection.On<int>("ReceiveCarDelete", (carId) => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var carToRemove = CarsRental.FirstOrDefault(c => c.Id == carId);
                        if (carToRemove != null)
                        {
                            CarsRental.Remove(carToRemove);
                        }
                    });
                });            
                _hubConnection.On<ObservableCollection<CarsRentalModel>>("ReceiveCars", (cars) => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        CarsRental.Clear();
                        foreach (var car in cars)
                        {
                            CarsRental.Add(car);
                        }
                    });
                });
                
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("GetCars");
            }
            catch (Exception ex)
            {
                _mainWindowViewModel.Notification("Server", "Error SignalR connection", true, false, 3, true);
                Console.WriteLine($"Error starting SignalR connection: {ex.Message}");
            }
        }
        public async Task StopConnection()
        {
            if (_hubConnection != null)
            {
                _hubConnection.Remove("ReceiveCarUpdate");
                _hubConnection.Remove("ReceiveCarDelete");
                _hubConnection.Remove("ReceiveCars");

                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }      
        }
    }
}