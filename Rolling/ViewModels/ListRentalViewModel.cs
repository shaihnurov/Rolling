using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Models;

namespace Rolling.ViewModels;

public class ListRentalViewModel : BaseViewModel
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    
    private ObservableCollection<RentalCarInfoDto> _listRental;
    private int _endRentalDay;
    
    public int EndRentalDay
    {
        get => _endRentalDay;
        set => SetProperty(ref _endRentalDay, value);
    }
    
    public ObservableCollection<RentalCarInfoDto> ListRental
    {
        get => _listRental;
        set => SetProperty(ref _listRental, value);
    }
    
    public ListRentalViewModel(MainWindowViewModel mainWindowViewModel) : base("rentalcarhub")
    {
        _mainWindowViewModel = mainWindowViewModel;

        ListRental = [];
    }
    
    public override async Task ConnectToSignalR()
    {
        try
        {
            await base.ConnectToSignalR();
            
            _hubConnection.On<ObservableCollection<RentalCarInfoDto>>("RentalListUser", cars => {
                Dispatcher.UIThread.Post(() =>
                {
                    ListRental.Clear();
                    foreach (var item in cars)
                    {
                        ListRental.Add(item);
                    }
                });
            });
            _hubConnection.On<string>("ErrorListUserRental", errorMessage => {
                Dispatcher.UIThread.Post(() =>
                {
                    _mainWindowViewModel.Notification("Server", $"{errorMessage}", true, false, 3, true);
                });
            });
            
            await _hubConnection.InvokeAsync("ListUserRental");
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