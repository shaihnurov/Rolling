using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels;

public class UserProfileViewModel : BaseViewModel
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    private string _address;
    private string _userName;
    private string _userEmail;
    private string _userAge;
    private int _userLevel;

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }
    public string UserEmail
    {
        get => _userEmail;
        set => SetProperty(ref _userEmail, value);
    }
    public string UserAge
    {
        get => _userAge;
        set => SetProperty(ref _userAge, value);
    }
    public int UserLevel
    {
        get => _userLevel;
        set => SetProperty(ref _userLevel, value);
    }

    public AsyncRelayCommand ExitAccountCommand { get; set; }
    
    public UserProfileViewModel(MainWindowViewModel mainWindowViewModel) : base("userprofilehub")
    {
        _mainWindowViewModel = mainWindowViewModel;

        _mainWindowViewModel.TryAgainLocationCommand = new AsyncRelayCommand(GetLocation);
        
        ExitAccountCommand = new AsyncRelayCommand(Exit);
    }
    
    private async Task GetLocation()
    {
        /*IsLoading = true;
        var location = await _geoLocationService.GetLocation();

        if (location != null)
        {
            Address = $"{location}";

            var userData = await UserDataStorage.GetUserData();
            if (userData != null)
            {
                userData.Location = location;
                await UserDataStorage.SaveUserData(userData);
            }
            
            _mainWindowViewModel.IsInfoBarVisible = false;
            _mainWindowViewModel.IsVisibleButtonInfoBar = false;
        }
        else
        {
            _mainWindowViewModel.Notification("Location", "Failed to get location", true, true, 3, false);
        }

        IsLoading = false;*/
    }
    public override async Task ConnectToSignalR()
    {
        try
        {
            await base.ConnectToSignalR();
            
            _hubConnection.On<UserModel, string>("ReturnCurrentUser", (userData, location) => {
                Dispatcher.UIThread.Post(() =>
                {
                    UserName = userData.Name;
                    UserEmail = userData.Email;
                    UserAge = userData.Age.ToString();
                    UserLevel = userData.Level;
                    Address = location;
                });
            });
            _hubConnection.On("ExitAccount", ()=> {
                Dispatcher.UIThread.Post(() =>
                {
                    _mainWindowViewModel.TitleText = "Auth";
                    _mainWindowViewModel.IsVisibleBtnUserAcc = false;
                    _mainWindowViewModel.IsVisibleBtnAuthOrReg = true;
                    _mainWindowViewModel.CurrentView = new LoginViewModel(_mainWindowViewModel);
                    
                    _mainWindowViewModel.Notification("Account", "You have successfully logged out of your account", true, false, 2, true);
                });
            });
            
            await _hubConnection.InvokeAsync("LoadUserData");
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
    private async Task Exit()
    {
        await _hubConnection.InvokeAsync("ExitAccount");
    }
}