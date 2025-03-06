namespace WeatherBotSolutionV2.Models
{
    public class WeatherResponse
    {
        public Main Main { get; set; }
        public List<Weather> Weather { get; set; }
        public string? Name { get; set; }

        public string GetWeatherSummary()
        {
            var temperature = Math.Round(Main.Temp);
            var description = Weather.FirstOrDefault()?.Description ?? "Невідомо";
            return $"Погода в місті {Name}: {description}, температура: {temperature}°C";
        }
    }

    public class Main
    {
        public float Temp { get; set; }
        public float Humidity { get; set; }
        public float Pressure { get; set; }
    }

    public class Weather
    {
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }
}
