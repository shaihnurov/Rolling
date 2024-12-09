using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private DialogService _dialogService;

        private object _currentView;
        
        public object CurrentView
        {
            get => _currentView;
            set
            {
                SetProperty(ref _currentView, value);

                if (_currentView is IServerConnectionHandler newServerConnectionHandler)
                {
                    newServerConnectionHandler.ConnectToSignalR();
                }
            }
        }
        
        private bool _isInfoBarVisible = false;
        private bool _isVisibleButtonInfoBar = false;
        private string _messageInfoBar;
        private string _titleTextInfoBar;
        private int _statusInfoBar;
        private bool _isVisibleBtnUserAcc;
        private string _titleText;
        
        public bool IsInfoBarVisible
        {
            get => _isInfoBarVisible;
            set => SetProperty(ref _isInfoBarVisible, value);
        }
        public bool IsVisibleButtonInfoBar
        {
            get => _isVisibleButtonInfoBar;
            set => SetProperty(ref _isVisibleButtonInfoBar, value);
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
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }
        
        private RegisterViewModel RegisterViewModel { get; set; }
        private LoginViewModel LoginViewModel { get; set; }
        private HomeViewModel HomeViewModel { get; set; }
        private UserProfileViewModel UserProfileViewModel { get; set; }
        private ListRentalViewModel ListRentalViewModel { get; set; }
        
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand UserProfileCommand { get; set; }
        public RelayCommand ListRentalCommand { get; set; }
        public AsyncRelayCommand SupportChatCommand { get; set; }
        
        public MainWindowViewModel()
        {
            TitleText = "Process Authentication";
            
            RegisterViewModel = new RegisterViewModel(this);
            LoginViewModel = new LoginViewModel(this);
            HomeViewModel = new HomeViewModel(this);
            UserProfileViewModel = new UserProfileViewModel(this);
            ListRentalViewModel = new ListRentalViewModel(this);
            
            HomeViewCommand = new RelayCommand(() => {
                CurrentView = HomeViewModel;
                TitleText = "Home";
            });
            UserProfileCommand = new RelayCommand(() => {
                CurrentView = UserProfileViewModel;
                TitleText = "Profile";
            });
            ListRentalCommand = new RelayCommand(() =>
            {
                CurrentView = ListRentalViewModel;
            });
            SupportChatCommand = new AsyncRelayCommand(OpenDialogSupport);

            CurrentView = LoginViewModel;
        }
        
        public async void Notification(string title, string message, bool visibleInfoBar, bool visibleBtnInfoBar, int statusCode, bool timeLife)
        {
            _dialogService = new DialogService();

            TitleTextInfoBar = title;
            MessageInfoBar = message;
            IsVisibleButtonInfoBar = visibleBtnInfoBar;
            IsInfoBarVisible = visibleInfoBar;
            StatusInfoBar = statusCode;

            if (timeLife)
            {
                await Task.Delay(3000);
                IsInfoBarVisible = false;
            }
        }
        private async Task OpenDialogSupport()
        {
            await _dialogService.ShowDialogAsync(this);
        }
    }
}