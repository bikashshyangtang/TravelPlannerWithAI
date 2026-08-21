using TravelPlanner.Domain.Enums;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.Trips;

public record TripResponse(
    int Id,
    string Destination,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Budget,
    BudgetLevel BudgetLevel,
    TripStatus Status,
    int DurationInDays,
    List<DayResponse> Days
);

public record DayResponse(
    int DayNumber,
    DateOnly Date,
    string? Summary,
    List<ActivityResponse> Activities
);

public record ActivityResponse(
    string Name,
    string? Description,
    TimeOnly? StartTime,
    int? DurationInMinutes,
    decimal? EstimatedCost,
    string? Location
);


public record TripResponseSummary(
    int Id,
    string? Destination,
    DateOnly StartDate,
    DateOnly EndDate,
    TripStatus Status,
    int DurationInDays
);


public static class TripResponseExtensions
{
    public static TripResponse ToTripResponse(this Trip trip)
    {
        return new TripResponse(
            trip.Id,
            trip.Destination ?? string.Empty,
            trip.StartDate,
            trip.EndDate,
            trip.Budget,
            trip.BudgetLevel,
            trip.Status,
            trip.DurationInDays,
            trip.ItineraryDays
                .OrderBy(day => day.DayNumber)
                .Select(day => new DayResponse(
                    day.DayNumber,
                    day.Date,
                    day.Summary,
                    day.Activities
                        .OrderBy(activity => activity.StartTime)
                        .Select(activity => new ActivityResponse(
                        activity.Name,
                        activity.Description,
                        activity.StartTime,
                        activity.DurationMinutes,
                        activity.EstimatedCost,
                        activity.Location
                )).ToList()
            )).ToList()
        );
    }
    public static TripResponseSummary ToSummaryResponse(this Trip trip) => new(
        trip.Id,
        trip.Destination,
        trip.StartDate,
        trip.EndDate,
        trip.Status,
        trip.DurationInDays);
}