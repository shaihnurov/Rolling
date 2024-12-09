using System;
using System.Threading.Tasks;
using Rolling.ViewModels;

namespace Rolling.Service;

public interface IDialogService
{
    Task<string?> ShowDialogAsync(MainWindowViewModel mainWindowViewModel, string mark, string model, Guid id, int year, string color, int horsePower, int mileage, double engine, string location, double price, bool status, byte[] image);
    Task<string?> ShowDialogAsync(MainWindowViewModel mainWindowViewModel);
}