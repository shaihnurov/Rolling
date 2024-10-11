using System;
using System.Threading.Tasks;
using Rolling.Models;

namespace Rolling.Service;

public interface IUserService
{
    public void UpdateUserData();
    public event Action UserDataChanged;
}