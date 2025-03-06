using Dapper;
using Microsoft.Data.SqlClient;
using WeatherBotSolutionV2.Data;

public class DbInitializer : IDbInitializer
{
    private readonly string _connectionString;

    public DbInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var createUsersTable = @"
        IF OBJECT_ID('Users', 'U') IS NULL
        CREATE TABLE Users (
            Id INT IDENTITY PRIMARY KEY,
            TelegramId BIGINT UNIQUE NOT NULL,
            UserName NVARCHAR(100),
            FirstName NVARCHAR(100),
            LastName NVARCHAR(100)
        );";
        await connection.ExecuteAsync(createUsersTable);

        var createWeatherHistoryTable = @"
        IF OBJECT_ID('WeatherHistory', 'U') IS NULL
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