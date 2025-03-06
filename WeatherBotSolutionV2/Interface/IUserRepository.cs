using WeatherBotSolutionV2.Models;
using WeatherBotSolutionV2.Models;

namespace WeatherBotSolutionV2.Data
{
    public interface IUserRepository
    {
        Task AddUserAsync(long telegramId, string userName, string firstName, string lastName);
        Task<MyUser?> GetUserByIdAsync(int userId);
        Task<MyUser?> GetUserByTelegramIdAsync(long telegramId);
        Task<IEnumerable<MyUser>> GetAllUsersAsync();
        Task<IEnumerable<WeatherHistory>> GetUserWeatherHistoryAsync(int userId);
    }
}
