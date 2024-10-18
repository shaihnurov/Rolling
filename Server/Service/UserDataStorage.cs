using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using Server.Model;

namespace Server.Service;

public class UserDataStorage
{
    private const string FilePath = "user-data.json";

    public static async Task SaveUserData(UserData userData)
    {
        var jsonString = JsonSerializer.Serialize(userData);
        await File.WriteAllTextAsync(FilePath, jsonString);
    }

    public static async Task<UserData> GetUserData()
    {
        if (File.Exists(FilePath))
        {
            string jsonString = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<UserData>(jsonString)!;
        }

        return null!;
    }

    public static void DeleteUserData()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}