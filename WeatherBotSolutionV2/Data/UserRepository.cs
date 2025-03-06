using Dapper;
using Microsoft.Data.SqlClient;
using WeatherBotSolutionV2.Data;
using WeatherBotSolutionV2.Models;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddUserAsync(long telegramId, string userName, string firstName, string lastName)
    {
        const string query = @"
        IF NOT EXISTS (SELECT 1 FROM Users WHERE TelegramId = @TelegramId)
        INSERT INTO Users (TelegramId, UserName, FirstName, LastName)
        VALUES (@TelegramId, @UserName, @FirstName, @LastName)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        var parameters = new { TelegramId = telegramId, UserName = userName, FirstName = firstName, LastName = lastName };
        await connection.ExecuteAsync(query, parameters).ConfigureAwait(false);
    }

    public async Task<MyUser?> GetUserByIdAsync(int userId)
    {
        const string query = "SELECT * FROM Users WHERE Id = @UserId";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<MyUser>(query, new { UserId = userId }).ConfigureAwait(false);
    }

    public async Task<MyUser?> GetUserByTelegramIdAsync(long telegramId)
    {
        const string query = "SELECT * FROM Users WHERE TelegramId = @TelegramId";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        return await connection.QuerySingleOrDefaultAsync<MyUser>(query, new { TelegramId = telegramId }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<MyUser>> GetAllUsersAsync()
    {
        const string query = "SELECT * FROM Users";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        return await connection.QueryAsync<MyUser>(query).ConfigureAwait(false);
    }

    public async Task<IEnumerable<WeatherHistory>> GetUserWeatherHistoryAsync(int userId)
    {
        const string query = @"
        SELECT TOP 100 Id, UserTelegramId, City, Temperature, WeatherDescription, RequestTime
        FROM WeatherHistory
        WHERE UserTelegramId = @UserId
        ORDER BY RequestTime DESC";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        return await connection.QueryAsync<WeatherHistory>(query, new { UserId = userId }).ConfigureAwait(false);
    }
}