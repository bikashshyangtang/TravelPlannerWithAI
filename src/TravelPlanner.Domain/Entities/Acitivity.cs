namespace TravelPlanner.Domain.Entities;
public class Activity
{
    public int Id{ get; set; }
    public required string Name{ get; set; }
    public string? Description{ get; set; }
    public TimeOnly? StartTime{ get; set; }
    public int? DurationMinutes{ get; set; }
    public decimal? EstimatedCost{ get; set; }
    public string? Location{ get; set; }
    public int ItineraryDayId{ get; set; }
    public ItineraryDay? ItineraryDay{ get; set; } = null;

}