using System;

namespace Rolling.Models;

public class RentalCarInfoDto
{
    public Guid Id { get; set; }
    public string Mark { get; set; }
    public string Model { get; set; }
    public int Years { get; set; }
    public string Color { get; set; }
    public int HorsePower { get; set; }
    public int Mileage { get; set; }
    public double Engine { get; set; }
    public double Price { get; set; }
    public string City { get; set; }
    public bool Status { get; set; }
    public byte[]? Image { get; set; }
    public DateTime EndDate { get; set; }
}