namespace TravelPlanner.Domain.Entities;
public class ItineraryDay
{
    public int Id { get; set; }
    public int DayNumber { get; set; }
    public DateOnly Date { get; set; }
    public string? Summary { get; set; }
    public int TripId { get; set; }
    public Trip? Trip { get; set; } = null;
    public List<Activity> Activities { get; set; } = [];
}