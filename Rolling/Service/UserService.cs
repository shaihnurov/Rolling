using System;
using System.Threading.Tasks;
using Rolling.Models;

namespace Rolling.Service;

public class UserService : IUserService
{
    public event Action? UserDataChanged;

    public void UpdateUserData()
    {
        UserDataChanged?.Invoke();
    }
}