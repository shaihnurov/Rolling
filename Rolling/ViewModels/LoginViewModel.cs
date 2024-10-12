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
                    _mainWindowViewModel.TitleTextInfoBar = "Auth";
                    _mainWindowViewModel.MessageInfoBar = "Please state your correct email";
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.StatusInfoBar = 3;
                    
                    Task.Run(async() =>
                    {
                        await Task.Delay(3000);
                        _mainWindowViewModel.IsInfoBarVisible = false;
                    });
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
            RegisterUserCommand = new AsyncRelayCommand(async() => {
                if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password))
                {
                    await AuthUser();
                }
                else
                {
                    _mainWindowViewModel!.TitleTextInfoBar = "Auth";
                    _mainWindowViewModel!.MessageInfoBar = "Please fill in all available fields";
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.StatusInfoBar = 2;

                    await Task.Delay(3000);
                    _mainWindowViewModel.IsInfoBarVisible = false;
                }
            });

            _mainWindowViewModel = mainWindowViewModel;
        }
        
        private async Task AuthUser()
        { 
            using (ApplicationContextDb db = new())
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
                        if (IsCheckedSaveData == true)
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
                        _mainWindowViewModel.UserService.UpdateUserData();

                        _mainWindowViewModel.IsVisibleBtnUserAcc = true;
                        _mainWindowViewModel.IsVisibleBtnAuthOrReg = false;
                        _mainWindowViewModel.TitleTextInfoBar = "Auth";
                        _mainWindowViewModel.MessageInfoBar = "Successful entry";
                        _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                        _mainWindowViewModel.IsInfoBarVisible = true;
                        _mainWindowViewModel.StatusInfoBar = 1;
                        
                        _mainWindowViewModel.CurrentView = new HomeViewModel();
                
                        await Task.Delay(3000);
                        _mainWindowViewModel.IsInfoBarVisible = false;
                    }
                    else
                    {
                        _mainWindowViewModel.TitleTextInfoBar = "Auth";
                        _mainWindowViewModel.MessageInfoBar = "Incorrect data. Please check the correctness of the entered data";
                        _mainWindowViewModel.IsInfoBarVisible = true;
                        _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                        _mainWindowViewModel.StatusInfoBar = 3;
                
                        await Task.Delay(3000);
                        _mainWindowViewModel.IsInfoBarVisible = false;
                    }
                }
                else
                {
                    _mainWindowViewModel.TitleTextInfoBar = "Auth";
                    _mainWindowViewModel.MessageInfoBar = "User not found. If you think this is an error, please contact support";
                    _mainWindowViewModel.IsInfoBarVisible = true;
                    _mainWindowViewModel.IsVisibleButtonInfoBar = false;
                    _mainWindowViewModel.StatusInfoBar = 3;
                
                    await Task.Delay(3000);
                    _mainWindowViewModel.IsInfoBarVisible = false;
                }
            }
        }
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        }
    }
}