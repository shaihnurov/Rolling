using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Models;
using Rolling.Service;
using Rolling.Views;

namespace Rolling.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private int _state = 0;
        private object _currentView;
        private string _btnRegOrAuthText;
        
        public object CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView is IServerConnectionHandler currentServerConnectionHandler)
                {
                    currentServerConnectionHandler.StopConnection();
                }
                
                SetProperty(ref _currentView, value);
                
                if (_currentView is IServerConnectionHandler newServerConnectionHandler)
                {
                    newServerConnectionHandler.ConnectToSignalR();
                }
            }
        }
        public string BtnRegOrAuthText
        {
            get => _btnRegOrAuthText;
            set => SetProperty(ref _btnRegOrAuthText, value);
        }
        
        private bool _isInfoBarVisible = false;
        private bool _isVisibleButtonInfoBar = false;
        private bool _isVisibleButtonAdmin = false;
        private string _messageInfoBar;
        private string _titleTextInfoBar;
        private int _statusInfoBar;
        private bool _isVisibleBtnAuthOrReg;
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
        public bool IsVisibleButtonAdmin
        {
            get => _isVisibleButtonAdmin;
            set => SetProperty(ref _isVisibleButtonAdmin, value);
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
        private AdminViewModel AdminViewModel { get; set; }
        
        public RelayCommand BtnRegOrAuthCommand { get; set; }
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand UserProfileCommand { get; set; }
        public AsyncRelayCommand TryAgainLocationCommand { get; set; }
        public RelayCommand AdminViewCommand { get; set; }
        
        public MainWindowViewModel()
        {
            BtnRegOrAuthText = "Do you have an account yet?";
            TitleText = "Procces Authentication";
            
            RegisterViewModel = new RegisterViewModel(this);
            LoginViewModel = new LoginViewModel(this);
            HomeViewModel = new HomeViewModel(this);
            UserProfileViewModel = new UserProfileViewModel(this);
            AdminViewModel = new AdminViewModel(this);
            
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
            AdminViewCommand = new RelayCommand(() => {
                CurrentView = AdminViewModel;
                TitleText = "Admin Controls";
            });
            
            CurrentView = LoginViewModel;
            IsVisibleBtnAuthOrReg = true;
        }
        
        public async void Notification(string title, string message, bool visibleInfoBar, bool visibleBtnInfoBar, int statusCode, bool timeLife)
        {
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
    }
}