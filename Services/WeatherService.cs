using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TokenGenerator.Services
{
    public record CurrentWeather(double Temperature, int WeatherCode, bool IsDay);

    public static class WeatherService
    {
        static readonly HttpClient _httpClient = new();

        public static async Task<CurrentWeather?> FetchAsync(double latitude, double longitude)
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude:F4}&longitude={longitude:F4}&current_weather=true";
            var result = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);
            var current = result?.CurrentWeather;

            return current is null ? null : new CurrentWeather(current.Temperature, current.WeatherCode, current.IsDay == 1);
        }

        public static string DescribeIcon(int code, bool isDay) => code switch
        {
            0 => isDay ? "☀️" : "🌙",
            1 or 2 or 3 => isDay ? "⛅" : "☁️",
            45 or 48 => "🌫️",
            51 or 53 or 55 or 56 or 57 => "🌦️",
            61 or 63 or 65 or 66 or 67 or 80 or 81 or 82 => "🌧️",
            71 or 73 or 75 or 77 or 85 or 86 => "❄️",
            95 or 96 or 99 => "⛈️",
            _ => "🌡️"
        };

        public static string DescribeCode(int code) => code switch
        {
            0 => "Clear sky",
            1 or 2 or 3 => "Partly cloudy",
            45 or 48 => "Fog",
            51 or 53 or 55 => "Drizzle",
            56 or 57 => "Freezing drizzle",
            61 or 63 or 65 => "Rain",
            66 or 67 => "Freezing rain",
            71 or 73 or 75 => "Snow",
            77 => "Snow grains",
            80 or 81 or 82 => "Rain showers",
            85 or 86 => "Snow showers",
            95 => "Thunderstorm",
            96 or 99 => "Thunderstorm with hail",
            _ => "Unknown"
        };

        class OpenMeteoResponse
        {
            [JsonPropertyName("current_weather")]
            public InnerCurrentWeather? CurrentWeather { get; set; }
        }

        class InnerCurrentWeather
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("weathercode")]
            public int WeatherCode { get; set; }

            [JsonPropertyName("is_day")]
            public int IsDay { get; set; }
        }
    }
}
