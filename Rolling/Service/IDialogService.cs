using System.Threading.Tasks;

namespace Rolling.Service;

public interface IDialogService
{
    Task<string?> ShowDialogAsync(string mark, string model, int id, int year, string color, int horsePower, int mileage, double engine, string location, double price, bool status);
}