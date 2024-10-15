using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using Avalonia.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using Rolling.Service;

namespace Rolling.ViewModels;

public class UserProfileViewModel : ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly IUserService _userService;
    private readonly GeoLocationService _geoLocationService;

    private string _address;
    private bool _isLoading;

    private string _userName;
    private string _userEmail;
    private string _userAge;
    private int _userLevel;

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }
    public string UserEmail
    {
        get => _userEmail;
        set => SetProperty(ref _userEmail, value);
    }
    public string UserAge
    {
        get => _userAge;
        set => SetProperty(ref _userAge, value);
    }
    public int UserLevel
    {
        get => _userLevel;
        set => SetProperty(ref _userLevel, value);
    }

    public RelayCommand ExitAccountCommand { get; set; }
    
    public UserProfileViewModel(MainWindowViewModel mainWindowViewModel, IUserService userService)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _geoLocationService = new GeoLocationService();
        _userService = userService;

        _mainWindowViewModel.TryAgainLocationCommand = new AsyncRelayCommand(GetLocation);

        _userService.UserDataChanged += async () => await LoadDataUser();
        Task.Run(async () => {
            await LoadDataUser();
            await GetLocation();
        });
        
        ExitAccountCommand = new RelayCommand(Exit);
    }
    
    private async Task GetLocation()
    {
        IsLoading = true;
        var location = await _geoLocationService.GetLocation();

        if (location != null)
        {
            Address = $"{location}";

            var userData = await UserDataStorage.GetUserData();
            if (userData != null)
            {
                userData.Location = location;
                await UserDataStorage.SaveUserData(userData);
            }
            
            _mainWindowViewModel.IsInfoBarVisible = false;
            _mainWindowViewModel.IsVisibleButtonInfoBar = false;
        }
        else
        {
            _mainWindowViewModel.TitleTextInfoBar = "Location";
            _mainWindowViewModel.MessageInfoBar = "Failed to get location";
            _mainWindowViewModel.IsInfoBarVisible = true;
            _mainWindowViewModel.IsVisibleButtonInfoBar = true;
            _mainWindowViewModel.StatusInfoBar = 3;
        }

        IsLoading = false;
    }
    private void Exit()
    {
        UserDataStorage.DeleteUserData();
        _mainWindowViewModel.TitleText = "Auth";
        _mainWindowViewModel.IsVisibleBtnUserAcc = false;
        _mainWindowViewModel.IsVisibleBtnAuthOrReg = true;
        _mainWindowViewModel.CurrentView = new LoginViewModel(_mainWindowViewModel);
    }
    private async Task LoadDataUser()
    {
        using (ApplicationContextDb db = new()) 
        {
            UserData userData = await UserDataStorage.GetUserData();
            var user = await db.UserModels.Where(s => s.Email == userData.Email).ToListAsync();

            foreach (var item in user)
            {
                UserName = item.Name;
                UserEmail = item.Email;
                UserAge = item.Age.ToString();
                UserLevel = item.Level;
            }
        }
    }
}