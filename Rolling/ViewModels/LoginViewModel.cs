using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;

namespace Rolling.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private bool _isCheckedSaveData;
        private bool _resultAuth;
        private string _email;
        private string _password;
        private Button _regBtn;

        private readonly MainWindowViewModel _mainWindowViewModel;
        public AsyncRelayCommand RegisterUserCommand { get; set; }

        public bool IsCheckedSaveData
        {
            get => _isCheckedSaveData;
            set => SetProperty(ref _isCheckedSaveData, value);
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
        
        private async Task AuthUser()
        { 
            /*using (ApplicationContextDb db = new())
            {
                var currentUser = await db.UserModels.AsNoTracking().Where(s => s.Email == Email).ToListAsync();

                if (currentUser.Count == 1)
                {
                    foreach (var item in currentUser)
                    {
                        _resultAuth = BCrypt.Net.BCrypt.Verify(Password, item.Password);
                    }

                    if (_resultAuth)
                    {
                        if (IsCheckedSaveData)
                        {
                            string token = TokenService.GenerateToken(Email);
                            var userData = new UserData
                            {
                                Token = token,
                                Email = Email,
                                ExpiryDate = DateTime.UtcNow.AddDays(7)
                            };
                            
                            await UserDataStorage.SaveUserData(userData);
                        }
                        else
                        {
                            var userData = new UserData
                            {
                                Email = Email
                            };

                            await UserDataStorage.SaveUserData(userData);
                        }

                        bool permission = await _mainWindowViewModel.CheckUserPermission(Email);
                        
                        if(permission)
                            _mainWindowViewModel.IsVisibleButtonAdmin = false;
                        else
                            _mainWindowViewModel.IsVisibleButtonAdmin = true;
                        
                        _mainWindowViewModel.UserService.UpdateUserData();

                        _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                        _mainWindowViewModel.IsVisibleBtnAuthOrReg = false;
                        
                        _mainWindowViewModel.Notification("Auth", "Successful entry", true, false, 1, true);
                        
                        _mainWindowViewModel.CurrentView = new HomeViewModel(_mainWindowViewModel);
                
                        await Task.Delay(3000);
                        _mainWindowViewModel.IsInfoBarVisible = false;
                    }
                    else
                    {
                        _mainWindowViewModel.Notification("Auth", "Incorrect data. Please check the correctness of the entered data", true, false, 3, true);
                    }
                }
                else
                {
                    _mainWindowViewModel.Notification("Auth", "User not found. If you think this is an error, please contact support", true, false, 3, true);
                }
            }*/
        }
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
    }
}