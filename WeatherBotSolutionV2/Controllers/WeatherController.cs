using Microsoft.AspNetCore.Mvc;
using WeatherBotSolutionV2.Services;

namespace WeatherBotSolutionV2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherService _weatherService;

        public WeatherController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        /// <summary>
        /// Get current weather for a specified city.
        /// </summary>
        /// <param name="city">The name of the city to fetch weather data for.</param>
        [HttpGet("current/{city}")]
        public async Task<IActionResult> GetWeatherAsync(string city)
        {
            var weatherData = await _weatherService.GetWeatherAsync(city);
            if (weatherData == null)
            {
                return NotFound("Weather data not found.");
            }
            return Ok(weatherData);
        }
    }
}
