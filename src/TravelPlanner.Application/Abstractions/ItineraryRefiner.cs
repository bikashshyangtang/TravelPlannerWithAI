using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.Abstractions;
public interface IItineraryRefiner
{
    Task<string> RefineAsync(Trip trip, string userMessage, CancellationToken cancellationToken);
}