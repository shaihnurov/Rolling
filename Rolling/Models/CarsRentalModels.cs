using CommunityToolkit.Mvvm.ComponentModel;

namespace Rolling.Models;

public class CarsRentalModels : ObservableObject
{
    private int _id;
    private string _mark;
    private string _model;
    private int _years;
    private string _color;
    private int _horsePower;
    private int _mileage;
    private double _engine;
    private double _price;
    private string _city;
    private bool _status;
    private byte[]? _image;

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
    public int Years
    {
        get => _years;
        set => SetProperty(ref _years, value);
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
    public double Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }
    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }
    public bool Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
    public byte[]? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }
}
