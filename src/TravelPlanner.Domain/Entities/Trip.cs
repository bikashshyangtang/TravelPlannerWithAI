using TravelPlanner.Domain.Enums;
namespace TravelPlanner.Domain.Entities;
public class Trip
{
    public int Id { get; set; }
    public string? Destination { get; set; } = null;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal Budget { get; set; }
    public BudgetLevel BudgetLevel { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Draft;
    public List<ItineraryDay> ItineraryDays{get; set;} = [];
    public List<ChatMessage> ChatMessages { get; set; } = [];
    public  int DurationInDays => EndDate.DayNumber - StartDate.DayNumber + 1;
    private Trip() { } // Private constructor for EF Core
    public Trip (string destination, DateOnly startDate, DateOnly endDate, decimal budget, BudgetLevel budgetLevel)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("Destination cannot be null or empty.", nameof(destination));
        }
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be later than end date.", nameof(endDate));
        }
        if (budget < 0)
        {
            throw new ArgumentException("Budget cannot be negative.", nameof(budget));
        }
        Destination = destination.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Budget = budget;
        BudgetLevel = budgetLevel;
    }
    public void MarkGenerating() => Status = TripStatus.Generating;
    public void MarkFailed() => Status = TripStatus.Failed;
    public void MarkComplete()
    {
        if(ItineraryDays == null || ItineraryDays.Count == 0)
        {
            throw new InvalidOperationException("Cannot mark trip as complete without itinerary days.");
        }
        Status = TripStatus.Complete;
    }
    public void AddDay(ItineraryDay day)
    {
        if (day == null)
        {
            throw new ArgumentNullException(nameof(day), "Itinerary day cannot be null.");
        }
        if (day.Date < StartDate || day.Date > EndDate)
        {
            throw new ArgumentOutOfRangeException(nameof(day), "Day falls outside the trip's date range.");
        }
        ItineraryDays.Add(day);
    }
}


