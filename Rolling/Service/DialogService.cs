using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Rolling.ViewModels;
using Rolling.Views;

namespace Rolling.Service;

public class DialogService : IDialogService
{
    public async Task<string?> ShowDialogAsync(MainWindowViewModel mainWindowViewModel, string mark, string model, Guid id, int year, string color, int horsePower, int mileage, double engine, string location, double price, bool status, byte[] image)
    {
        var dialogWindow = new DialogWindow();
        dialogWindow.DataContext = new DialogWindowViewModel(mainWindowViewModel, dialogWindow, mark, model, id, year, color, horsePower, mileage, engine, location, price, status, image);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return await dialogWindow.ShowDialog<string>(desktop.MainWindow);
        }

        return null;
    }

    public async Task<string?> ShowDialogAsync(MainWindowViewModel mainWindowViewModel)
    {
        var dialogWindow = new DialogWindow();
        dialogWindow.DataContext = new DialogWindowViewModel(mainWindowViewModel, dialogWindow);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return await dialogWindow.ShowDialog<string>(desktop.MainWindow);
        }

        return null;
    }
}