using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace WeatherBotSolutionV2.Data
{
    public class DbInitializer
    {
        private readonly string _connectionString;

        public DbInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeDatabaseAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var createUsersTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                CREATE TABLE Users (
                    Id INT IDENTITY PRIMARY KEY,
                    TelegramId BIGINT UNIQUE NOT NULL,
                    UserName NVARCHAR(100),
                    FirstName NVARCHAR(100),
                    LastName NVARCHAR(100)
                );";
                await connection.ExecuteAsync(createUsersTable);

                var createWeatherHistoryTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WeatherHistory' AND xtype='U')
                CREATE TABLE WeatherHistory (
                    Id INT IDENTITY PRIMARY KEY,
                    UserTelegramId INT NOT NULL,
                    City NVARCHAR(100),
                    Temperature FLOAT,
                    WeatherDescription NVARCHAR(255),
                    RequestTime DATETIME DEFAULT GETDATE(),
                    FOREIGN KEY (UserTelegramId) REFERENCES Users(Id) ON DELETE CASCADE
                );";
                await connection.ExecuteAsync(createWeatherHistoryTable);
            }
        }
    }
}
