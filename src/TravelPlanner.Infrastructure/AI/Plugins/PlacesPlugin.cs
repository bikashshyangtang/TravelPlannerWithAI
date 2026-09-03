using TravelPlanner.Application.Abstractions;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace TravelPlanner.Infrastructure.AI.Plugins
{
    public class PlacesPlugin
    {
        private readonly IPlaceService _placeService;

        public PlacesPlugin(IPlaceService placeService)
        {
            _placeService = placeService;
        }

        [KernelFunction("search_places")]
        [Description("Searches for real attractions, restaurants, and points of interest at a destination. " +
                 "Call this before adding activities to an itinerary so that every suggested place " +
                 "actually exists and has a real address.")]
        public async Task<string> SearchPlacesAsync([Description("City Name or area Name")]string placeName, [Description("List of interests")]string interests, CancellationToken cancellationToken)
        {
            var response = await _placeService.SearchAsync(placeName, interests, cancellationToken);
            return string.Join("\n", response.Select(p => $"- {p.Name} ({p.Category ?? "places"}) - {p.Address ?? "Unknown address"}")); // Placeholder for actual implementation
        }
    }
}   