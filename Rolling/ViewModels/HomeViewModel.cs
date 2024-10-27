using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels;

public class HomeViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private ObservableCollection<CarsRentalModels> _rentalCars;

    public ObservableCollection<CarsRentalModels> RentalCars
    {
        get => _rentalCars;
        set => SetProperty(ref _rentalCars, value);
    }

    public AsyncRelayCommand<object> OpenDialogCommand { get; set; }

    public HomeViewModel(MainWindowViewModel mainWindowViewModel) : base("rentalcarhub")
    {
        _dialogService = new DialogService();
        _mainWindowViewModel = mainWindowViewModel;
        RentalCars = [];

        OpenDialogCommand = new AsyncRelayCommand<object>(OpenDialog);
    }

    private async Task OpenDialog(object parameter)
    {
        if (parameter is CarsRentalModels selectedCar)
        {
            await _dialogService.ShowDialogAsync(_mainWindowViewModel, selectedCar.Mark, selectedCar.Model, selectedCar.Id, selectedCar.Years, selectedCar.Color, selectedCar.HorsePower, selectedCar.Mileage, selectedCar.Engine, selectedCar.City, selectedCar.Price, selectedCar.Status, selectedCar.Image!);
        }
    }
    public override async Task ConnectToSignalR()
    {
        try
        {
            await base.ConnectToSignalR();

            _hubConnection.On<CarsRentalModels>("UpdateCars", (car) => {
                Dispatcher.UIThread.Post(() =>
                {
                    var existingCar = RentalCars.FirstOrDefault(c => c.Id == car.Id);
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
                        RentalCars.Add(car);
                    }
                });
            });
            _hubConnection.On<Guid>("DeleteCars", (carId) => {
                Dispatcher.UIThread.Post(() =>
                {
                    var carToRemove = RentalCars.FirstOrDefault(c => c.Id == carId);
                    if (carToRemove != null)
                    {
                        RentalCars.Remove(carToRemove);
                    }
                });
            });
            _hubConnection.On<ObservableCollection<CarsRentalModels>>("RentalCarsList", (cars) => {
                Dispatcher.UIThread.Post(() =>
                {
                    RentalCars.Clear();
                    foreach (var car in cars)
                    {
                        RentalCars.Add(car);
                    }
                });
            });
            _hubConnection.On("RentalCarsListInvalidLocation", ()=> {
                Dispatcher.UIThread.Post(() =>
                {
                    _mainWindowViewModel.Notification("Application", "Failed to detect the cars around you", true, false, 3, true);
                });
            });
            
            await _hubConnection.InvokeAsync("GetCars");
        }
        catch (HttpRequestException ex)
        {
            _mainWindowViewModel.Notification("Server", "Failed to connect to the server. Please check your network.", true, false, 3, true);
            Console.WriteLine($"HttpRequestException: {ex.Message}");
        }
        catch (SocketException ex)
        {
            _mainWindowViewModel.Notification("Network", "Network error occurred while connecting to the server.", true, false, 3, true);
            Console.WriteLine($"SocketException: {ex.Message}");
        }
        catch (HubException ex)
        {
            _mainWindowViewModel.Notification("Server", "Error occurred with the SignalR hub connection.", true, false, 3, true);
            Console.WriteLine($"HubException: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _mainWindowViewModel.Notification("Error", "An error occurred in the application. Please try again.", true, false, 3, true);
            Console.WriteLine($"InvalidOperationException: {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            _mainWindowViewModel.Notification("Timeout", "Connection to the server timed out. Please try again.", true, false, 3, true);
            Console.WriteLine($"TimeoutException: {ex.Message}");
        }
        catch (Exception ex)
        {
            _mainWindowViewModel.Notification("Error", "An unexpected error occurred.", true, false, 3, true);
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}
