using TravelPlanner.Application.Abstractions;
using TravelPlanner.Application.Trips;

namespace TravelPlanner.Infrastructure.AI;
public class FakeItineraryGenerator : IItineraryGenerator
{
    public Task<GeneratedItinerary> GenerateAsync(TripCriteria criteria, CancellationToken cancellationToken)
    {
        var days = new List<GeneratedDay>();
        var daysCount = criteria.EndDate.DayNumber - criteria.StartDate.DayNumber + 1;

        for(int i = 0; i < daysCount; i++)
        {
            var date = criteria.StartDate.AddDays(i);
            var activities = new List<GeneratedActivity>
            {
                new("Morning Walk", $"A stroll through {criteria.Destination}'s scenic areas.", new TimeOnly(7, 0), 60, "Central Park", 0m),
                new("Lunch at Local Diner", $"Enjoy local cuisine in {criteria.Destination}.", new TimeOnly(12, 0), 90, "Downtown Diner", 15m),
                new("Afternoon Museum Visit", null, new TimeOnly(14, 0), 120, null, 20m),
            };

            days.Add(new GeneratedDay(
                DayNumber: i + 1,
                Date: date,
                Summary: $"Day {i + 1} in {criteria.Destination}",
                Activities: activities
            ));
           
        }
        return Task.FromResult(new GeneratedItinerary(days));
    }
}