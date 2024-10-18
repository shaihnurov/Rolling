using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Service;

namespace Rolling.ViewModels
{
    public class LoginViewModel : ObservableObject, IServerConnectionHandler
    {
        private bool _isCheckedSaveData;
        private bool _isLoad;
        private bool _isLoggedIn;
        private string _email;
        private string _password;
        private Button _regBtn;

        private readonly MainWindowViewModel _mainWindowViewModel;
        private HubConnection _hubConnection;
        
        public AsyncRelayCommand RegisterUserCommand { get; set; }

        public bool IsCheckedSaveData
        {
            get => _isCheckedSaveData;
            set => SetProperty(ref _isCheckedSaveData, value);
        }
        public bool IsLoad
        {
            get => _isLoad;
            set => SetProperty(ref _isLoad, value);
        }
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
        }
        public string Email
        {
            get => _email;
            set
            {
                if (IsValidEmail(value))
                {
                    SetProperty(ref _email, value);
                }
                else
                {
                    _mainWindowViewModel.Notification("Auth", "Please state your correct email", true, false, 3, true);
                }
            }
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public Button RegBtn
        {
            get => _regBtn;
            set => SetProperty(ref _regBtn, value);
        }
        
        public LoginViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            IsLoggedIn = false;
            IsLoad = true;
            _mainWindowViewModel.IsVisibleBtnUserAcc = false;
            _mainWindowViewModel.IsVisibleBtnAuthOrReg = false;
            
            RegisterUserCommand = new AsyncRelayCommand(async() => {
                if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
                {
                    await AuthUser();
                }
                else
                {
                    _mainWindowViewModel.Notification("Auth", "Please fill in all available fields", true, false, 2, true);
                }
            });
        }
        
        public async void ConnectToSignalR()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder().WithUrl("https://localhost:7160/authhub").Build();
                
                _hubConnection.On<bool>("AuthUser", permission => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if(permission)
                            _mainWindowViewModel.IsVisibleButtonAdmin = false;
                        else
                            _mainWindowViewModel.IsVisibleButtonAdmin = true;
                        
                        _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                        _mainWindowViewModel.IsVisibleBtnAuthOrReg = false;
                        
                        _mainWindowViewModel.Notification("Auth", "Successful entry", true, false, 1, true);
                        
                        _mainWindowViewModel.CurrentView = new HomeViewModel(_mainWindowViewModel);
                    });
                });
                _hubConnection.On<string>("AuthErrorNotFound", result => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainWindowViewModel.Notification("Auth", result, true, false, 3, true);
                    });
                });
                _hubConnection.On<string>("AuthUserInvalidPass", result => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainWindowViewModel.Notification("Auth", result, true, false, 3, true);
                    });
                });
                _hubConnection.On<bool>("AuthToken", permission => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if(permission)
                            _mainWindowViewModel.IsVisibleButtonAdmin = false;
                        else
                            _mainWindowViewModel.IsVisibleButtonAdmin = true;
                        
                        _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                        _mainWindowViewModel.IsVisibleBtnAuthOrReg = false;
                        IsLoggedIn = true;
                        IsLoad = false;                        
                        _mainWindowViewModel.Notification("Auth", "Successful entry", true, false, 1, true);
                        
                        _mainWindowViewModel.CurrentView = new HomeViewModel(_mainWindowViewModel);
                    });
                });
                _hubConnection.On<string>("AuthInvalidToken", result => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoggedIn = true;
                        IsLoad = false;
                        _mainWindowViewModel.Notification("Auth", result, true, false, 3, true);
                    });
                });
                _hubConnection.On<string>("AuthTokenNotFound", result => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoggedIn = true;
                        IsLoad = false;
                    });
                });
                
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("AuthToken");
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
        private async Task AuthUser()
        { 
            await _hubConnection.InvokeAsync("AuthUser", Email, Password, IsCheckedSaveData);
        }
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
    }
}