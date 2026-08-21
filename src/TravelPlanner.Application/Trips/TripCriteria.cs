using TravelPlanner.Domain.Enums;

namespace TravelPlanner.Application.Trips;

public record TripCriteria(
    string Destination, 
    DateOnly StartDate, 
    DateOnly EndDate, 
    decimal Budget, 
    BudgetLevel BudgetLevel,
    List<string> Interests
);