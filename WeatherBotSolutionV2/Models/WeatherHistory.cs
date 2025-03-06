namespace WeatherBotSolutionV2.Models
{
    public class WeatherHistory
    {
        public int Id { get; set; }
        public long UserTelegramId { get; set; }
        public string? City { get; set; }
        public double Temperature { get; set; }
        public string? WeatherDescription { get; set; }
        public DateTime RequestTime { get; set; }
    }
}
