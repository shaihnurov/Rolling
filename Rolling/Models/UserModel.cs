namespace Rolling.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? Age { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Level { get; set; }
    public string Permission { get; set; }
    public double Balance { get; set; }
}