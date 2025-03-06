using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using Newtonsoft.Json;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using WeatherBotSolutionV2.Models;

namespace WeatherBotSolutionV2.Services
{
    public class BotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _connectionString;
        private readonly string _apiKey;

        public BotService(IConfiguration configuration)
        {
            string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? "CHANGE_TO_YOUR_TOKEN";
            _botClient = new TelegramBotClient(botToken);
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _apiKey = configuration["WeatherToken"];
            Console.WriteLine("BotService initialized.");
        }

        public async Task StartBotAsync()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            using CancellationTokenSource cts = new();
            var updateHandler = new UpdateHandler(_botClient, _connectionString, _apiKey);

            _botClient.StartReceiving(updateHandler, receiverOptions, cts.Token);

            Console.WriteLine("Bot started MTF...");
        }
    }

    public class UpdateHandler : IUpdateHandler
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _connectionString;
        private readonly string _apiKey;
        private static Dictionary<long, string> UserCityInput = new();

        public UpdateHandler(TelegramBotClient botClient, string connectionString, string apiKey)
        {
            _botClient = botClient;
            _connectionString = connectionString;
            _apiKey = apiKey;
            Console.WriteLine("UpdateHandler initialized.");
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received update: {update.Type}");

            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessageUpdateAsync(botClient, update.Message, cancellationToken);
                    Console.WriteLine("__________________________________________________________________________");
                    break;

                case UpdateType.CallbackQuery:
                    await HandleCallbackQueryUpdateAsync(botClient, update.CallbackQuery, cancellationToken);
                    Console.WriteLine("__________________________________________________________________________");
                    break;

                default:
                    Console.WriteLine($"Unhandled update type: {update.Type}");
                    Console.WriteLine("__________________________________________________________________________");
                    break;
            }
        }

        private async Task HandleMessageUpdateAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Processing message: {message.Text}");

            if (message.Text?.ToLower().Contains("привіт") == true)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Слава Україні !!! ❤️❤️❤️ {message.Chat.FirstName}", cancellationToken: cancellationToken);
                Console.WriteLine("Responded to greeting.");
               
                return;
            }

            switch (message.Text)
            {
                case string text when text.StartsWith("/start"):
                    Console.WriteLine("Handling /start command.");
                    await ShowMenuAsync(message.Chat.Id, cancellationToken);
                    break;

                case string text when text.StartsWith("/weather"):
                    Console.WriteLine("Handling /weather command.");
                    await HandleWeatherCommandAsync(botClient, message, cancellationToken);
                    break;

                case string text when UserCityInput.ContainsKey(message.Chat.Id):
                    Console.WriteLine("Handling city input.");
                    await HandleCityInputAsync(botClient, message, cancellationToken);
                    break;

                default:
                    Console.WriteLine($"Unknown command: {message.Text}");
                    await botClient.SendTextMessageAsync(
                        message.Chat.Id,
                        "Егей.... я таких команд ще не вивчив! 😅",
                        cancellationToken: cancellationToken
                    );
                    await ShowMenuAsync(message.Chat.Id, cancellationToken);
                    Console.WriteLine("__________________________________________________________________________");
                    break;
            }
        }

        private async Task HandleWeatherCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var city = message.Text.Substring(9).Trim();
            Console.WriteLine($"Fetching weather for city: {city}");

            try
            {
                var weatherResponse = await GetWeatherFromApiAsync(city);

                var temperature = Math.Round(weatherResponse.Main.Temp);
                var description = weatherResponse.Weather.FirstOrDefault()?.Description ?? "Не відомо";

                var weatherMessage = $"Погода в місті {city}: {description}, температура: {temperature}°C";
                await botClient.SendTextMessageAsync(message.Chat.Id, weatherMessage, cancellationToken: cancellationToken);

                SaveWeatherHistory(message, city, temperature, description);
                Console.WriteLine($"Weather saved for city: {city}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather: {ex.Message}");
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Помилка при отриманні погоди для міста {city}: {ex.Message}", cancellationToken: cancellationToken);
                SaveWeatherHistory(message, city, 0, ex.Message);
            }

            await ShowMenuAsync(message.Chat.Id, cancellationToken);
        }

        private async Task HandleCityInputAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var city = message.Text.Trim();
            Console.WriteLine($"Handling city input: {city}");

            try
            {
                var weatherResponse = await GetWeatherFromApiAsync(city);

                var temperature = Math.Round(weatherResponse.Main.Temp);
                var description = weatherResponse.Weather.FirstOrDefault()?.Description ?? "Не відомо";

                var weatherMessage = $"Погода в місті {city}: {description}, температура: {temperature}°C";
                await botClient.SendTextMessageAsync(message.Chat.Id, weatherMessage, cancellationToken: cancellationToken);

                SaveWeatherHistory(message, city, temperature, description);
                UserCityInput.Remove(message.Chat.Id);
                Console.WriteLine($"Weather saved for city: {city}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather: {ex.Message}");
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Помилка при отриманні погоди для міста {city}: {ex.Message}", cancellationToken: cancellationToken);
                UserCityInput.Remove(message.Chat.Id);
            }

            await ShowMenuAsync(message.Chat.Id, cancellationToken);
        }

        private async Task HandleCallbackQueryUpdateAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Handling callback query: {callbackQuery.Data}");

            switch (callbackQuery.Data)
            {
                case "weather":
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введіть місто для отримання погоди:", cancellationToken: cancellationToken);
                    UserCityInput[callbackQuery.Message.Chat.Id] = "awaiting city";
                    Console.WriteLine("User requested weather.");
                    break;

                case "help":
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Доступні команди: \n/weather {місто} - Погода для міста", cancellationToken: cancellationToken);
                    await ShowMenuAsync(callbackQuery.Message.Chat.Id, cancellationToken);
                    Console.WriteLine("User requested help.");
                    break;

                case "about":
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Цей бот створений для отримання погоди. Розробник: Andriy Polianyk.", cancellationToken: cancellationToken);
                    await ShowMenuAsync(callbackQuery.Message.Chat.Id, cancellationToken);
                    Console.WriteLine("User requested about.");
                    break;
            }

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Команду вибрано", cancellationToken: cancellationToken);
        }

        private async Task ShowMenuAsync(long chatId, CancellationToken cancellationToken)
        {
            Console.WriteLine("Showing menu.");
            var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                new[] {
                    InlineKeyboardButton.WithCallbackData("Погода", "weather"),
                    InlineKeyboardButton.WithCallbackData("Допомога", "help")
                },
                new[] {
                    InlineKeyboardButton.WithCallbackData("Про розробника", "about")
                }
            });

            await _botClient.SendTextMessageAsync(chatId, "Виберіть команду:", replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        }

        public async Task<WeatherResponse> GetWeatherFromApiAsync(string city)
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";
            Console.WriteLine($"Fetching weather from API for city: {city}");

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
        }

        private void SaveWeatherHistory(Message message, string city, double temperature, string description)
        {
            long telegramId = message.From.Id;
            string userName = message.From.Username;
            string firstName = message.From.FirstName;
            string lastName = message.From.LastName;

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    var userId = connection.QuerySingleOrDefault<int?>(
                        "SELECT Id FROM Users WHERE TelegramId = @TelegramId",
                        new { TelegramId = telegramId }
                    );

                    if (userId == null)
                    {
                        var insertUserQuery = @"
                        INSERT INTO Users (TelegramId, UserName, FirstName, LastName) 
                        VALUES (@TelegramId, @UserName, @FirstName, @LastName);
                        SELECT SCOPE_IDENTITY();";

                        userId = connection.ExecuteScalar<int>(insertUserQuery, new
                        {
                            TelegramId = telegramId,
                            UserName = userName,
                            FirstName = firstName,
                            LastName = lastName
                        });

                        Console.WriteLine($"User {telegramId} added to Users table.");
                    }

                    var insertWeatherQuery = @"
                    INSERT INTO WeatherHistory (UserTelegramId, City, Temperature, WeatherDescription, RequestTime) 
                    VALUES (@UserTelegramId, @City, @Temperature, @WeatherDescription, @RequestTime)";

                    var weatherParameters = new
                    {
                        UserTelegramId = userId,
                        City = city,
                        Temperature = temperature,
                        WeatherDescription = description,
                        RequestTime = DateTime.UtcNow
                    };

                    connection.Execute(insertWeatherQuery, weatherParameters);
                    Console.WriteLine($"Weather history saved for user {userId}: {city}, {temperature}°C, {description}");
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    throw;
                }
            }
        }
    }
}