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
    public class LoginViewModel : BaseViewModel
    {
        private bool _isCheckedSaveData;
        private bool _isLoad;
        private bool _isLoggedIn;
        private string _email;
        private string _password;
        private Button _regBtn;
        private bool _isVisibleReconnectBtn = false;
        
        private readonly MainWindowViewModel _mainWindowViewModel;
        
        public AsyncRelayCommand RegisterUserCommand { get; set; }
        public AsyncRelayCommand ReconnectCommand { get; set; }
        public RelayCommand CreateAccountCommand { get; set; }

        public bool IsCheckedSaveData
        {
            get => _isCheckedSaveData;
            set => SetProperty(ref _isCheckedSaveData, value);
        }
        public bool IsVisibleReconnectBtn
        {
            get => _isVisibleReconnectBtn;
            set => SetProperty(ref _isVisibleReconnectBtn, value);
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
        
        public LoginViewModel(MainWindowViewModel mainWindowViewModel) : base("authhub")
        {
            _mainWindowViewModel = mainWindowViewModel;

            IsLoggedIn = false;
            _mainWindowViewModel.IsVisibleBtnUserAcc = false;
            
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
            CreateAccountCommand = new RelayCommand(CreateAccount);
            ReconnectCommand = new AsyncRelayCommand(ConnectToSignalR);
        }
        
        public override async Task ConnectToSignalR()
        {
            if (_hubConnection != null && 
                (_hubConnection.State == HubConnectionState.Connected || 
                 _hubConnection.State == HubConnectionState.Connecting || 
                 _hubConnection.State == HubConnectionState.Reconnecting))
            {
                Console.WriteLine("Соединение уже установлено или в процессе восстановления. Подождите...");
                return;
            }
            
            IsVisibleReconnectBtn = false;
            IsLoad = true;
            try
            {
                if (_hubConnection != null)
                {
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                }
                
                await base.ConnectToSignalR();
                
                _hubConnection.On<bool>("AuthUser", permission => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                        
                        _mainWindowViewModel.Notification("Auth", "Successful entry", true, false, 1, true);
                        
                        _mainWindowViewModel.CurrentView = new UserProfileViewModel(_mainWindowViewModel);
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
                        _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                        IsLoggedIn = true;
                        IsLoad = false;                        
                        _mainWindowViewModel.Notification("Auth", "Successful entry", true, false, 1, true);
                        
                        _mainWindowViewModel.CurrentView = new UserProfileViewModel(_mainWindowViewModel);
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
                
                await _hubConnection.InvokeAsync("AuthToken");
            }
            catch (HttpRequestException ex)
            {
                _mainWindowViewModel.Notification("Server", "Failed to connect to the server. Please check your network.", true, false, 3, true);
                IsVisibleReconnectBtn = true;
                IsLoad = false;
                Console.WriteLine($"HttpRequestException: {ex.Message}");
            }
            catch (SocketException ex)
            {
                _mainWindowViewModel.Notification("Network", "Network error occurred while connecting to the server.", true, false, 3, true);
                IsVisibleReconnectBtn = true;
                IsLoad = false;
                Console.WriteLine($"SocketException: {ex.Message}");
            }
            catch (HubException ex)
            {
                _mainWindowViewModel.Notification("Server", "Error occurred with the SignalR hub connection.", true, false, 3, true);
                IsVisibleReconnectBtn = true;
                IsLoad = false;
                Console.WriteLine($"HubException: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                _mainWindowViewModel.Notification("Error", "An error occurred in the application. Please try again.", true, false, 3, true);
                IsVisibleReconnectBtn = true;
                IsLoad = false;
                Console.WriteLine($"InvalidOperationException: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                _mainWindowViewModel.Notification("Timeout", "Connection to the server timed out. Please try again.", true, false, 3, true);
                IsVisibleReconnectBtn = true;
                IsLoad = false;
                Console.WriteLine($"TimeoutException: {ex.Message}");
            }
            catch (Exception ex)
            {
                _mainWindowViewModel.Notification("Error", "An unexpected error occurred.", true, false, 3, true);
                IsVisibleReconnectBtn = true;
                IsLoad = false;
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
        private void CreateAccount()
        {
            _mainWindowViewModel.TitleText = "Register";
            _mainWindowViewModel.CurrentView = new RegisterViewModel(_mainWindowViewModel);
        }
        private async Task AuthUser()
        { 
            await _hubConnection.InvokeAsync("AuthUser", Email, Password, IsCheckedSaveData);
        }
        private static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
    }
}