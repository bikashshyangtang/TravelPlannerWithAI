using TravelPlanner.Application.Abstractions;
using System.Net.Http.Json;
using System.Text.Json.Serialization;


namespace TravelPlanner.Infrastructure.ExternalApis
{
    public class OpenMateoWeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public OpenMateoWeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetWeatherForecastAsync(string location, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
        {
            var coordinates = await GetCoordinatesAsync(location, cancellationToken);
            if(coordinates is null) return null; // Handle the case where coordinates could not be retrieved

            return null; // Placeholder for actual implementation
            // Implementation for fetching weather forecast
        }

        private async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string location, CancellationToken cancellationToken)
        {
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location)}&count=1";
            var response = await _httpClient.GetFromJsonAsync<CoordinateResponse>(url, cancellationToken);
            var result = response?.Results?.FirstOrDefault();
            return result is null ? null : (result.Latitude, result.Longitude);
            // Handle the case where no results are found
            // Implementation for fetching coordinates from Open-Meteo API
            //var daysAhead = startDate.DayNumber -
        }

        private record CoordinateResponse([property: JsonPropertyName("results")] List<CoordinateResult> Results);
        private record CoordinateResult([property: JsonPropertyName("latitude")] double Latitude, [property: JsonPropertyName("longitude")] double Longitude);
        private record WeatherForecastResponse([property: JsonPropertyName("daily")] DailyData? Daily);
        private record DailyData([property: JsonPropertyName("time")] List<string> Time, [property: JsonPropertyName("temperature_2m_max")] List<double> TemperatureMax, [property: JsonPropertyName("temperature_2m_min")] List<double> TemperatureMin, [property: JsonPropertyName("precipitation_probability_max")] List<double> PrecipitationProbability);
    }
}   