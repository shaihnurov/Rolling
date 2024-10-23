using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Rolling.ViewModels;

public class DialogWindowViewModel : ObservableObject
{
    private Window _window;
    private int _year;
    private string _color;
    private int _horsePower;
    private int _mileage;
    private double _engine;
    private string _location;
    private double _price;
    private string _mark;
    private string _model;
    private string _status;
    private int _id;
    private bool _confirmBtnEnabled;
    private bool _isAvailable;
    
    public bool IsAvailable
    {
        get => _isAvailable;
        set => SetProperty(ref _isAvailable, value);
    }
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    public string Mark
    {
        get => _mark;
        set => SetProperty(ref _mark, value);
    }
    public string Model
    {
        get => _model;
        set => SetProperty(ref _model, value);
    }
    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }
    public string Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }
    public int HorsePower
    {
        get => _horsePower;
        set => SetProperty(ref _horsePower, value);
    }
    public int Mileage
    {
        get => _mileage;
        set => SetProperty(ref _mileage, value);
    }
    public double Engine
    {
        get => _engine;
        set => SetProperty(ref _engine, value);
    }
    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }
    public double Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
    public bool ConfirmBtnEnabled
    {
        get => _confirmBtnEnabled;
        set => SetProperty(ref _confirmBtnEnabled, value);
    }

    public RelayCommand ConfirmCommand { get; set; }
    public RelayCommand CancelCommand { get; set; }

    public DialogWindowViewModel(Window window, string mark, string model, int id, int year, string color, int horsePower, int mileage, double engine, string location, double price, bool status)
    {
        _window = window;
        
        LoadDataCar(mark, model, id, year, color, horsePower, mileage, engine, location, price, status);
        ConfirmCommand = new RelayCommand(Confrim);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void LoadDataCar(string mark, string model, int id, int year, string color, int horsePower, int mileage, double engine, string location, double price, bool status)
    {
        Id = id;
        Mark = mark;
        Model = model;
        Year = year;
        Color = color;
        HorsePower = horsePower;
        Mileage = mileage;
        Engine = engine;
        Location = location;
        Price = price;
        IsAvailable = status;
        if (IsAvailable)
        {
            Status = "Currently not available for rent";
            ConfirmBtnEnabled = false;
        }
        else
        {
            Status = "Available for rent";
            ConfirmBtnEnabled = true;
        }
    }
    private void Cancel()
    {
        _window.Close(null);
    }
    private void Confrim()
    {
        
    }
}