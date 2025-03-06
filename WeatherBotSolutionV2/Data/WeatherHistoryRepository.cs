using Dapper;
using Microsoft.Data.SqlClient;
using WeatherBotSolutionV2.Models;

namespace WeatherBotSolutionV2.Data
{
    public class WeatherHistoryRepository
    {
        private readonly string _connectionString;

        public WeatherHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<IEnumerable<WeatherHistory>> GetWeatherHistoryAsync(long userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM WeatherHistory WHERE UserTelegramId = @UserTelegramId";
                    return await connection.QueryAsync<WeatherHistory>(query, new { UserTelegramId = userId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching weather history.", ex);
            }
        }

        public async Task AddWeatherHistoryAsync(WeatherHistory weatherHistory)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "INSERT INTO WeatherHistory (UserTelegramId, City, Temperature, WeatherDescription, RequestTime) " +
                                "VALUES (@UserTelegramId, @City, @Temperature, @WeatherDescription, @RequestTime)";
                    var parameters = new
                    {
                        weatherHistory.UserTelegramId,
                        weatherHistory.City,
                        weatherHistory.Temperature,
                        weatherHistory.WeatherDescription,
                        weatherHistory.RequestTime
                    };
                    await connection.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding weather history.", ex);
            }
        }
    }
}
