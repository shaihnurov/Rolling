using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Service;

namespace Rolling.ViewModels
{
    public class RegisterViewModel : ObservableObject, IServerConnectionHandler
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
        private HubConnection _hubConnection;

        public AsyncRelayCommand RegisterUserCommand { get; set; }
        public AsyncRelayCommand ConfirmCodeRegCommand { get; set; }

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
        
        public RegisterViewModel(MainWindowViewModel mainWindowViewModel)
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
        }

        private async Task BtnSendCodeUser()
        {
            await _hubConnection.InvokeAsync("CheckUserEmail", Name, Email);
        }
        private async Task RegisterUser()
        {
            await _hubConnection.InvokeAsync("RegisterUser", Name, int.Parse(Age), Email, Password, Code, _verifycode);
        }
        private bool IsNumeric(string age)
        {
            return !string.IsNullOrEmpty(age) && age.All(char.IsDigit);
        }
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
        public async void ConnectToSignalR()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder().WithUrl("https://localhost:7160/registerhub").Build();
                
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
                        _mainWindowViewModel.IsVisibleBtnAuthOrReg = false;
                    
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
                
                await _hubConnection.StartAsync();
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