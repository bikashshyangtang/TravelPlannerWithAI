using TravelPlanner.Application.Abstractions;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.Trips.PlanTrip;

public class PlanTripHandler
{
    private readonly ITripRepository _tripRepository;
    private readonly IItineraryGenerator _itineraryGenerator;
    public PlanTripHandler(ITripRepository tripRepository, IItineraryGenerator itineraryGenerator)
    {
        _tripRepository = tripRepository;
        _itineraryGenerator = itineraryGenerator;
    }
    public async Task<Trip> HandleTripAsync(TripCriteria criteria, CancellationToken cancellationToken)
    {
        var trip = new Trip(
            criteria.Destination,
            criteria.StartDate,
            criteria.EndDate,
            criteria.Budget,
            criteria.BudgetLevel
        );

        trip.MarkGenerating();

        var genaretedItinerary = await _itineraryGenerator.GenerateAsync(criteria, cancellationToken);
        foreach (var day in genaretedItinerary.Days)
        {
            var itineraryDay = new ItineraryDay()
            {
                DayNumber = day.DayNumber,
                Date = day.Date,
                Summary = day.Summary,
                Activities = day.Activities.Select(activity => new Activity
                {
                    Name = activity.Name,
                    Description = activity.Description,
                    StartTime = activity.StartTime,
                    DurationMinutes = activity.DurationMinutes,
                    Location = activity.Location,
                    EstimatedCost = activity.EstimatedCost
                }).ToList()
            };
            trip.AddDay(itineraryDay);
        }
        trip.MarkComplete();

        await _tripRepository.AddAsync(trip, cancellationToken);
        await _tripRepository.SaveChangesAsync(cancellationToken);

        return trip;
    }
}