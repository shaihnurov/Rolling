using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels
{
    public class AdminViewModel : ObservableObject, IClosableConnection
    {
        private HubConnection _connection;
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
            
            CarsRental = new ObservableCollection<CarsRentalModel>();
            InitializeConnection();
            BtnSaveChangedCommand = new AsyncRelayCommand(SaveChanges);
        }

        private async void InitializeConnection()
        {
            try
            {
                _connection = new HubConnectionBuilder().WithUrl("https://localhost:7160/carhub").Build();

                _connection.On<ObservableCollection<CarsRentalModel>>("ReceiveCars", cars =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        CarsRental.Clear();
                        foreach (var car in cars)
                        {
                            CarsRental.Add(car);
                        }
                    });
                });

                await _connection.StartAsync();
                await LoadCars();
            }
            catch (HttpRequestException ex)
            {
                _mainWindowViewModel.Notification("Server Error", $"{ex.StatusCode} {ex.Message}", true, false, 3, true);
            }
            catch (SocketException ex)
            {
                _mainWindowViewModel.Notification("Server Error(socket)", $"{ex.SocketErrorCode} {ex.Message}", true, false, 3, true);
            }
        }
        private async Task LoadCars()
        {
            await _connection.InvokeAsync("GetCars");

        }
        private async Task SaveChanges()
        {
            if (CarsRental.Count != 0)
            {
                await _connection.InvokeAsync("SaveChangesCars", CarsRental);
            }
        }
        public async Task CloseConnectionAsync()
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}