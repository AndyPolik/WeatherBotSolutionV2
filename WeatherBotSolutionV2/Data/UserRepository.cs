using Dapper;
using Microsoft.Data.SqlClient;
using WeatherBotSolutionV2.Models;
using WeatherBotSolutionV2.Models.WeatherBotSolutionV2.Models;

namespace WeatherBotSolutionV2.Data
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
          
        }
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddUserAsync(long telegramId, string userName, string firstName, string lastName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var existingUser = await GetUserByTelegramIdAsync(telegramId);
                    if (existingUser != null)
                    {
                        return;
                    }

                    var query = "INSERT INTO Users (TelegramId, UserName, FirstName, LastName) VALUES (@TelegramId, @UserName, @FirstName, @LastName)";
                    var parameters = new { TelegramId = telegramId, UserName = userName, FirstName = firstName, LastName = lastName };
                    await connection.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding user to database.", ex);
            }
        }

        public async Task<MyUser> GetUserById(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM Users WHERE Id = @UserId";
                    return await connection.QuerySingleOrDefaultAsync<MyUser>(query, new { UserId = userId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching user by ID.", ex);
            }
        }

        public async Task<MyUser> GetUserByTelegramIdAsync(long telegramId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM Users WHERE TelegramId = @TelegramId";
                    return await connection.QuerySingleOrDefaultAsync<MyUser>(query, new { TelegramId = telegramId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching user by Telegram ID.", ex);
            }
        }

        public async Task<IEnumerable<MyUser>> GetAllUsers()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM Users";  
                    return await connection.QueryAsync<MyUser>(query);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all users.", ex);
            }
        }

        public async Task<IEnumerable<WeatherHistory>> GetUserWeatherHistory(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT TOP 100 Id, UserTelegramId, City, Temperature, WeatherDescription, RequestTime FROM WeatherHistory WHERE UserTelegramId = @UserId ORDER BY RequestTime DESC";

 
                    return await connection.QueryAsync<WeatherHistory>(query, new { UserId = userId });
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching weather history.", ex);
            }
        }
    }
}
