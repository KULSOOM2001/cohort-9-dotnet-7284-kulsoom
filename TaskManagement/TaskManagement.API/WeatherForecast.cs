namespace TaskManagement.API
{
    public class WeatherForecast
    {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => (int)(TemperatureC * 9.0 / 5.0 + 32);
        public string? Summary { get; set; }
    }
}