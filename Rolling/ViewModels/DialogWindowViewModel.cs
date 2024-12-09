using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rolling.Service;

namespace Rolling.ViewModels;

public class DialogWindowViewModel : ObservableObject
{
    private readonly Window _window;
    private object _currentView;
    private readonly MainWindowViewModel _mainWindowViewModel;
    
    private readonly InfoRentalCatViewModel _infoRentalCatViewModel;
    private readonly SupportChatViewModel _supportChatViewModel;


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

    public DialogWindowViewModel(MainWindowViewModel mainWindowViewModel, Window window, string mark, string model, Guid id, int year, string color, int horsePower, int mileage, double engine, string location, double price, bool status, byte[] image)
    {
        _window = window;
        _mainWindowViewModel = mainWindowViewModel;

        _infoRentalCatViewModel = new InfoRentalCatViewModel(window, mark, model, id, year, color, horsePower, mileage, engine, location, price, status, image, _mainWindowViewModel, this);
        CurrentView = _infoRentalCatViewModel;
    }

    public DialogWindowViewModel(MainWindowViewModel mainWindowViewModel, Window window)
    {
        _window = window;
        _mainWindowViewModel = mainWindowViewModel;

        _supportChatViewModel = new SupportChatViewModel(mainWindowViewModel, window);
        CurrentView = _supportChatViewModel;
    }
}