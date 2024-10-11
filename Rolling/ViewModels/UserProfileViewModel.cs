using System.Linq;
using System.Threading.Tasks;
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

    private string _userName;
    private string _userEmail;
    private string _userAge;

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

    public AsyncRelayCommand ExitAccountCommand { get; set; }
    
    public UserProfileViewModel(MainWindowViewModel mainWindowViewModel, IUserService userService)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _userService = userService;

        _userService.UserDataChanged += async () => await LoadDataUser();
        Task.Run(async () => await LoadDataUser());

        ExitAccountCommand = new AsyncRelayCommand(Exit);
    }

    private async Task Exit()
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
            }
        }
    }
}