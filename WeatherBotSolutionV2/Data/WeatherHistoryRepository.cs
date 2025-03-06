using Dapper;
using Microsoft.Data.SqlClient;
using WeatherBotSolutionV2.Data;
using WeatherBotSolutionV2.Models;

public class WeatherHistoryRepository : IWeatherHistoryRepository
{
    private readonly string _connectionString;

    public WeatherHistoryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<WeatherHistory>> GetWeatherHistoryAsync(long userId)
    {
        const string query = @"
        SELECT Id, UserTelegramId, City, Temperature, WeatherDescription, RequestTime
        FROM WeatherHistory
        WHERE UserTelegramId = @UserTelegramId";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<WeatherHistory>(query, new { UserTelegramId = userId });
    }

    public async Task AddWeatherHistoryAsync(WeatherHistory weatherHistory)
    {
        const string query = @"
        INSERT INTO WeatherHistory (UserTelegramId, City, Temperature, WeatherDescription, RequestTime)
        VALUES (@UserTelegramId, @City, @Temperature, @WeatherDescription, @RequestTime)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

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