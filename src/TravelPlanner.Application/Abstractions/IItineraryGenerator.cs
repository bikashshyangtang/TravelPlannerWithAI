using TravelPlanner.Application.Trips;

namespace TravelPlanner.Application.Abstractions;
public interface IItineraryGenerator
{
    Task<GeneratedItinerary> GenerateAsync(TripCriteria criteria, CancellationToken cancellationToken);
}