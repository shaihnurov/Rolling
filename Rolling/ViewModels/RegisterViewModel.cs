using System;
using System.Linq;
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
    public class RegisterViewModel : BaseViewModel
    {
        private string _name;
        private string _age;
        private string _email;
        private string _password;
        private string _code;
        private string _verifycode;
        private Button _regBtn;
        private bool _isVisibleUserData = true;
        private bool _isVisibleInputCode;
        
        private readonly MainWindowViewModel _mainWindowViewModel;

        public AsyncRelayCommand RegisterUserCommand { get; set; }
        public AsyncRelayCommand ConfirmCodeRegCommand { get; set; }
        public RelayCommand SignInAccountCommand { get; set; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
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
                    _mainWindowViewModel.Notification("Register", "Please state your correct email", true, false, 3, true);
                }
            }
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        public string Age
        {
            get => _age;
            set
            {
                if (IsNumeric(value))
                {
                    SetProperty(ref _age, value);
                }
                else
                {
                    _mainWindowViewModel.Notification("Register", "Please state your correct age", true, false, 3, true);
                }
            }
        }
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }
        public bool IsVisibleUserData
        {
            get => _isVisibleUserData;
            set => SetProperty(ref _isVisibleUserData, value);
        }
        public bool IsVisibleInputCode
        {
            get => _isVisibleInputCode;
            set => SetProperty(ref _isVisibleInputCode, value);
        }
        public Button RegBtn
        {
            get => _regBtn;
            set => SetProperty(ref _regBtn, value);
        }
        
        public RegisterViewModel(MainWindowViewModel mainWindowViewModel) : base("registerhub")
        {
            _mainWindowViewModel = mainWindowViewModel;
            
            RegisterUserCommand = new AsyncRelayCommand(async() => {
                if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Age) && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
                {
                    await BtnSendCodeUser();
                }
                else
                {
                    _mainWindowViewModel.Notification("Register", "Please fill in all available fields", true, false, 2, true);
                }
            });
            ConfirmCodeRegCommand = new AsyncRelayCommand(RegisterUser);
            SignInAccountCommand = new RelayCommand(SignInAccount);
        }
        
        public override async Task ConnectToSignalR()
        {
            try
            {
                await base.ConnectToSignalR();
                
                _hubConnection.On<string>("CheckUserEmail", verifycode => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsVisibleInputCode = true;
                        IsVisibleUserData = false;
                        _verifycode = verifycode;
                    });
                });
                _hubConnection.On("RegisterUser", () => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                    
                        _mainWindowViewModel.Notification("Register", "Registration successfully completed", true, false, 1, true);
                    
                        _mainWindowViewModel.CurrentView = new HomeViewModel(_mainWindowViewModel);
                    });
                });
                _hubConnection.On<string>("RegisterError", result => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _mainWindowViewModel.Notification("Register", result, true, false, 3, true);
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
        private void SignInAccount()
        {
            _mainWindowViewModel.TitleText = "Auth";
            _mainWindowViewModel.CurrentView = new LoginViewModel(_mainWindowViewModel);
        }
        private async Task BtnSendCodeUser()
        {
            await _hubConnection.InvokeAsync("CheckUserEmail", Name, Email);
        }
        private async Task RegisterUser()
        {
            await _hubConnection.InvokeAsync("RegisterUser", Name, int.Parse(Age), Email, Password, Code, _verifycode);
        }
        private static bool IsNumeric(string age)
        {
            return !string.IsNullOrEmpty(age) && age.All(char.IsDigit);
        }
        private static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
    }
}