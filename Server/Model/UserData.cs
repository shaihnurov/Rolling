using System;

namespace Server.Model;

public class UserData
{
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
}