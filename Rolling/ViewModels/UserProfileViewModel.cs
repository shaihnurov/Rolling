using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels;

public class UserProfileViewModel : ObservableObject, IServerConnectionHandler
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private HubConnection _hubConnection;

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
    
    public UserProfileViewModel(MainWindowViewModel mainWindowViewModel)
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
    private async Task Exit()
    {
        await _hubConnection.InvokeAsync("ExitAccount");
    }
    public async void ConnectToSignalR()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder().WithUrl("https://localhost:7160/userprofilehub").Build();
            
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
            
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync("LoadUserData");
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