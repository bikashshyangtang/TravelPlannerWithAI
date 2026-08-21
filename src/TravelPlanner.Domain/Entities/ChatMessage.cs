using TravelPlanner.Domain.Enums;
namespace TravelPlanner.Domain.Entities;
public class ChatMessage
{
    public int Id{ get; set; }
    public required string Content { get; set; }
    public MessageRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public int TripId { get; set; }
    public Trip? Trip { get; set; }
}