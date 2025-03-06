using Newtonsoft.Json;
using WeatherBotSolutionV2.Data;
using WeatherBotSolutionV2.Models;
using Telegram.Bot;
using Microsoft.Extensions.Configuration;

namespace WeatherBotSolutionV2.Services
{
    public class WeatherService
    {
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5/weather";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TelegramBotClient _botClient;
        private readonly IUserRepository _userRepository;

        public WeatherService(IHttpClientFactory httpClientFactory, TelegramBotClient botClient, IUserRepository userRepository, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _botClient = botClient;
            _userRepository = userRepository;
            _apiKey = configuration["WeatherToken"];
        }

        public async Task<WeatherData> GetWeatherAsync(string city)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrl}?q={city}&appid={_apiKey}&units=metric";

            try
            {
                var response = await client.GetStringAsync(url);
                var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

                if (weatherResponse != null)
                {
                    return new WeatherData
                    {
                        City = city,
                        Description = weatherResponse.Weather[0].Description,
                        Temperature = weatherResponse.Main.Temp
                    };
                }
            }
            catch (Exception ex) { 

                Console.WriteLine($"Error fetching weather data: {ex.Message}");
            }

            return null;
        }

        public async Task SendWeatherToAllUsersAsync(string city)
        {
            var weatherData = await GetWeatherAsync(city);
            if (weatherData == null)
            {
                return;
            }

            var users = await _userRepository.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return;
            }

            string weatherMessage = $"Weather in {city}: {weatherData.Description}, {weatherData.Temperature}°C.";

            foreach (var user in users)
            {
                try
                {
                    await _botClient.SendTextMessageAsync(user.TelegramId, weatherMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to user {user.TelegramId}: {ex.Message}");
                }
            }
        }
    }
}
