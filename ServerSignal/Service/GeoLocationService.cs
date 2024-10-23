using Newtonsoft.Json.Linq;

namespace ServerSignal.Service
{
    public class GeoLocationService
    {
        private const string Token = "5ed4f5c70ff6af";
        private readonly HttpClient _httpClient;

        public GeoLocationService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<string> GetLocation()
        {
            string url = $"https://ipinfo.io/json?token={Token}";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                if (json["city"] != null && json["region"] != null && json["country"] != null)
                {
                    string city = json["city"]!.ToString();
                    string region = json["region"]!.ToString();
                    string country = json["country"]!.ToString();

                    string location = $"{city}, {region}, {country}";

                    return location;
                }else
                    return "Не удалось получить местоположение";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка HTTP-запроса: {ex.Message}");
                return $"Ошибка запроса: {ex.Message}";
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Превышено время ожидания запроса.");
                return "Превышено время ожидания запроса.";
            }
        }
    }
}