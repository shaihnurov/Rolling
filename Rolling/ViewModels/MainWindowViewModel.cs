using System;
using System.Linq;
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Media;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Rolling.Models;
using Tmds.DBus.Protocol;

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
        
        private RegisterViewModel RegisterViewModel { get; set; }
        private LoginViewModel LoginViewModel { get; set; }
        
        public RelayCommand BtnRegOrAuthCommand { get; set; }

        public MainWindowViewModel()
        {
            BtnRegOrAuthText = "Do you have an account yet?";
            
            RegisterViewModel = new RegisterViewModel(this);
            LoginViewModel = new LoginViewModel(this);

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
        }
    }
}