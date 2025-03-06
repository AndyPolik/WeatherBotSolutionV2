using Microsoft.AspNetCore.Mvc;
using WeatherBotSolutionV2.Data;
using WeatherBotSolutionV2.Models;
using Telegram.Bot;
using Newtonsoft.Json;

namespace WeatherBotSolutionV2.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly TelegramBotClient _botClient;
        private readonly string  _weatherToken ;

        public UserController(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            string botToken = configuration["TelegramBotToken"];
            _botClient = new TelegramBotClient(botToken);
            _weatherToken = configuration["WeatherToken"];
        }

        /// <summary>
        /// Get History
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserHistory(int userId)
        {
            var history = await _userRepository.GetUserWeatherHistoryAsync(userId);
            if (history == null || !history.Any())
            {
                return NotFound($"No weather history found for user with ID {userId}");
            }
            return Ok(history);
        }

        /// <summary>
        /// Send weather information to all users or specific user.
        /// </summary>
        [HttpPost("sendWeatherToAll")]
        public async Task<IActionResult> SendWeatherToAll([FromBody] WeatherMessageRequest request)
        {
            // Якщо є UserId, то отримуємо одного користувача, інакше всіх користувачів
            var users = request.UserId.HasValue
                ? new[] { await _userRepository.GetUserByIdAsync(request.UserId.Value) }
                : await _userRepository.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return NotFound("No users found to send the message.");
            }

            foreach (var user in users)
            {
                var weatherResponse = await GetWeatherFromApiAsync("Київ");
                var weatherSummary = weatherResponse?.GetWeatherSummary();

                if (weatherSummary != null)
                {
                    await _botClient.SendTextMessageAsync(user.TelegramId, weatherSummary);
                }
            }

            return Ok(new { Message = "Weather message sent successfully" });
        }

        /// <summary>
        /// Get weather data from the API for a specified city.
        /// </summary>
        /// <param name="city">The city to fetch weather data for.</param>
        [HttpGet("weather/{city}")]
        public async Task<WeatherResponse> GetWeatherFromApiAsync(string city)
        {
            var apiKey = _weatherToken;  
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
        }

    }
}
