using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherBotSolutionV2.Services;
using WeatherBotSolutionV2.Data;
using Telegram.Bot;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WeatherBot Service API",
        Version = "v1",
        Description = "API для WeatherBot.",
        Contact = new OpenApiContact
        {
            Name = "Andriy Polianyk",
            Email = "andypolianyk@gmail.com"
        }
    });
});

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<WeatherService>();
builder.Services.AddSingleton<BotService>();

builder.Services.AddSingleton<UserRepository>(sp =>
    new UserRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DbInitializer>(sp =>
    new DbInitializer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<TelegramBotClient>(sp =>
    new TelegramBotClient(builder.Configuration["TelegramBotToken"]));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeDatabaseAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); 
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherBot API V1"); 
        c.RoutePrefix = string.Empty; 
    });
}

app.MapControllers(); 

var botService = app.Services.GetRequiredService<BotService>();
await botService.StartBotAsync();

await app.RunAsync();
