using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        public IUserService UserService => _userService;
        
        private int _state = 0;
        private object _currentView;
        private string _btnRegOrAuthText;
        
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }
        public string BtnRegOrAuthText
        {
            get => _btnRegOrAuthText;
            set => SetProperty(ref _btnRegOrAuthText, value);
        }
        
        private bool _isInfoBarVisible = false;
        private string _messageInfoBar;
        private string _titleTextInfoBar;
        private int _statusInfoBar;
        private bool _isVisibleBtnAuthOrReg = true;
        private bool _isVisibleBtnUserAcc;
        private string _titleText;
        
        public bool IsInfoBarVisible
        {
            get => _isInfoBarVisible;
            set => SetProperty(ref _isInfoBarVisible, value);
        }
        public string MessageInfoBar
        {
            get => _messageInfoBar;
            set => SetProperty(ref _messageInfoBar, value);
        }
        public string TitleTextInfoBar
        {
            get => _titleTextInfoBar;
            set => SetProperty(ref _titleTextInfoBar, value);
        }
        public int StatusInfoBar
        {
            get => _statusInfoBar;
            set => SetProperty(ref _statusInfoBar, value);
        }
        public bool IsVisibleBtnUserAcc
        {
            get => _isVisibleBtnUserAcc;
            set => SetProperty(ref _isVisibleBtnUserAcc, value);
        }
        public bool IsVisibleBtnAuthOrReg
        {
            get => _isVisibleBtnAuthOrReg;
            set => SetProperty(ref _isVisibleBtnAuthOrReg, value);
        }
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }
        
        private RegisterViewModel RegisterViewModel { get; set; }
        private LoginViewModel LoginViewModel { get; set; }
        private HomeViewModel HomeViewModel { get; set; }
        private UserProfileViewModel UserProfileViewModel { get; set; }
        
        public RelayCommand BtnRegOrAuthCommand { get; set; }
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand UserProfileCommand { get; set; }

        public MainWindowViewModel(IUserService userService)
        {
            _userService = userService;
            CheckUserToken();
            
            BtnRegOrAuthText = "Do you have an account yet?";
            TitleText = "Procces Authentication";
            
            RegisterViewModel = new RegisterViewModel(this);
            LoginViewModel = new LoginViewModel(this);
            HomeViewModel = new HomeViewModel();
            UserProfileViewModel = new UserProfileViewModel(this, _userService);

            CurrentView = LoginViewModel;

            BtnRegOrAuthCommand = new RelayCommand(() => {
                if (_state == 0)
                {
                    CurrentView = RegisterViewModel;
                    BtnRegOrAuthText = "You don't have an account yet?";
                    _state++;
                }else if (_state == 1)
                {
                    CurrentView = LoginViewModel;
                    BtnRegOrAuthText = "Do you have an account yet?";
                    _state--;
                }
            });
            HomeViewCommand = new RelayCommand(() => {
                CurrentView = HomeViewModel;
                TitleText = "Home";
            });
            UserProfileCommand = new RelayCommand(() => {
                CurrentView = UserProfileViewModel;
                TitleText = "Profile";
            });
        }

        private async void CheckUserToken()
        {
            UserData storedUserData = await UserDataStorage.GetUserData();

            if (storedUserData != null && !string.IsNullOrEmpty(storedUserData.Token))
            {
                var claimsPrincipal = TokenService.ValidateToken(storedUserData.Token);

                if (claimsPrincipal != null)
                {
                    IsVisibleBtnUserAcc = true;
                    IsVisibleBtnAuthOrReg = false;
                    CurrentView = HomeViewModel;
                    TitleText = "Home";
                }
                else
                {
                    UserDataStorage.DeleteUserData();
                    IsVisibleBtnUserAcc = false;
                    IsVisibleBtnAuthOrReg = true;
                    CurrentView = LoginViewModel;
                }
            }
            else
            {
                IsVisibleBtnUserAcc = false;
                IsVisibleBtnAuthOrReg = true;
                CurrentView = LoginViewModel;
            }
        }
    }
}