using System.ComponentModel.DataAnnotations;

namespace ServerSignal.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Enter Email")]
    [EmailAddress(ErrorMessage = "Invalid format email")]
    public string Email {get;set;}
    [Required(ErrorMessage = "Enter Password")]
    [DataType(DataType.Password)]
    public string Password {get;set;}
}