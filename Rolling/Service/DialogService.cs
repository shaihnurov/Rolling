using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Rolling.ViewModels;
using Rolling.Views;

namespace Rolling.Service;

public class DialogService : IDialogService
{
    public async Task<string?> ShowDialogAsync(string mark, string model, int id, int year, string color, int horsePower, int mileage, double engine, string location, double price, bool status)
    {
        var dialogWindow = new DialogWindow();
        dialogWindow.DataContext = new DialogWindowViewModel(dialogWindow, mark, model, id, year, color, horsePower, mileage, engine, location, price, status);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return await dialogWindow.ShowDialog<string>(desktop.MainWindow);
        }

        return null;
    }
}