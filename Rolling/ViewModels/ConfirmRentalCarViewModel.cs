using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Rolling.ViewModels
{
    public class ConfirmRentalCarViewModel : BaseViewModel
    {
        private Window _window;
        private readonly MainWindowViewModel _mainWindowViewModel;

        private string _location;
        private double _price;
        private string _mark;
        private string _model;
        private Guid _id;
        private byte[] _image;
        private string _rentalDays = "1";
        private double _totalPrice;
    
        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public string Mark
        {
            get => _mark;
            set => SetProperty(ref _mark, value);
        }
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }
        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }
        public double Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }
        public Byte[] Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        public string RentalDays
        {
            get => _rentalDays;
            set
            {
                if (IsNumeric(value))
                {
                    SetProperty(ref _rentalDays, value);
                    CalculateTotalPrice();
                }
                else
                {
                    _mainWindowViewModel.Notification("", "Please provide the correct date value", true, false, 2, true);
                }
            }
        }
        public double TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }
        
        public AsyncRelayCommand ConfirmCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        
        public ConfirmRentalCarViewModel(MainWindowViewModel mainWindowViewModel, Window window, string mark, string model, Guid id, string location, double price, byte[] image) : base("confrimrentalcar")
        {
            _mainWindowViewModel = mainWindowViewModel;
            _window = window;

            LoadDataCar(mark, model, id, location, price, image);
            TotalPrice = Price;

            ConfirmCommand = new AsyncRelayCommand(Confrim);
            CancelCommand = new RelayCommand(Cancel);
        }
        private void LoadDataCar(string mark, string model, Guid id, string location, double price, byte[] image)
        {
            Id = id;
            Mark = mark;
            Model = model;
            Location = location;
            Price = price;
            Image = image;
        }
        private void Cancel()
        {
            _window.Close(null);
        }
        public override async Task ConnectToSignalR()
        {
            try
            {
                await base.ConnectToSignalR();
                
                _hubConnection.On("SuccessfulPaymentRental", () => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainWindowViewModel.Notification("Payment", $"Successful car payment - {Mark} {Model} for {RentalDays} days", true, false, 1, true);
                        _window.Close(null);
                    });
                });
                _hubConnection.On<string>("ErrorPayment", errorMessage => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainWindowViewModel.Notification("", $"{errorMessage}", true, false, 3, true);
                        _window.Close(null);
                    });
                });
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
        private async Task Confrim()
        {
            await _hubConnection.InvokeAsync("PaymentRental", TotalPrice, Id, RentalDays);
        }
        private void CalculateTotalPrice()
        {
            if (int.TryParse(RentalDays, out int days))
            {
                TotalPrice = days * Price;
            }
            else
            {
                TotalPrice = 0;
            }
        }
        private static Boolean IsNumeric(string value)
        {
            return int.TryParse(value, out var result);
        }
    }
}