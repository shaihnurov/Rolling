using System;
using System.Net.Http;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Controls;
using Newtonsoft.Json.Linq;
using Rolling.Models;

namespace Rolling.Service;

public class GeoLocationService
{
    private const string Token = "5ed4f5c70ff6af";
    private readonly HttpClient _httpClient;

    public GeoLocationService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GetLocation()
    {
        var userData = await UserDataStorage.GetUserData();
        if(userData.Location == null)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        
            string url = $"https://ipinfo.io/json?token={Token}";

            try
            {
                var responce = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(responce);
            
                if (json["city"] != null && json["region"] != null && json["country"] != null)
                {
                    string city = json["city"]!.ToString();
                    string region = json["region"]!.ToString();
                    string country = json["country"]!.ToString();
                    
                    string location = $"{city}, {region}, {country}";
                    
                    return location;
                }

                MessageBox.Show("Не удалось получить местоположение.");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка запроса: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("Превышено время ожидания запроса.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
        }
        return userData.Location;
    }
}