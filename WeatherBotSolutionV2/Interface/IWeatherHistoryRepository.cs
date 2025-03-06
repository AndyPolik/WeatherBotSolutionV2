using WeatherBotSolutionV2.Models;

namespace WeatherBotSolutionV2.Data
{
    public interface IWeatherHistoryRepository
    {
        Task<IEnumerable<WeatherHistory>> GetWeatherHistoryAsync(long userId);
        Task AddWeatherHistoryAsync(WeatherHistory weatherHistory);
    }
}
