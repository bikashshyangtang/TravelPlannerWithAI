using TravelPlanner.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace TravelPlanner.Infrastructure.ExternalApis
{
    public class FourSquarePlaceService : IPlaceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FourSquarePlaceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["FourSquare:ApiKey"] ?? throw new InvalidOperationException("FourSquare:API key is not configured.");
        }

        public async Task<List<PlaceResult>> SearchAsync(string placeName, string interestQuery, CancellationToken cancellationToken)
        {
            var query = $"{placeName} {interestQuery}".Trim();
            // Construct the request URI for the Foursquare Places API
            //var requestUri = $"https://api.foursquare.com/places/search?query={Uri.EscapeDataString(interestQuery)}&near={Uri.EscapeDataString(placeName)}&limit=10";
            var requestUri = $"https://places-api.foursquare.com/places/search" +
                 $"?query={Uri.EscapeDataString(interestQuery)}" +
                 $"&near={Uri.EscapeDataString(placeName)}" +
                 $"&limit=5";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("X-Places-Api-Version", "2025-06-17"); // Example version, adjust as needed
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            //var content = await response.Content.ReadAsStringAsync(cancellationToken);
            //var placesResponse = System.Text.Json.JsonSerializer.Deserialize<FoursquarePlacesResponse>(content);
            var placesResponse = await response.Content.ReadFromJsonAsync<FoursquarePlacesResponse>(cancellationToken: cancellationToken);

            return placesResponse?.Results.Select(r => new PlaceResult(r.Name, r.Categories?.FirstOrDefault()?.Name, r.Location?.FormattedAddress)).ToList() ?? new List<PlaceResult>();
            //return null; // Placeholder for actual implementation
        }

        private record FoursquarePlacesResponse([property: JsonPropertyName("results")] List<FoursquarePlace> Results);
        private record FoursquarePlace(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("location")] FoursquareLocation? Location,
            [property: JsonPropertyName("categories")] List<FoursquareCategory>? Categories
        );

        private record FoursquareLocation(
            [property: JsonPropertyName("formatted_address")] string? FormattedAddress
        );

        private record FoursquareCategory(
            [property: JsonPropertyName("name")] string Name
        );
    }
}