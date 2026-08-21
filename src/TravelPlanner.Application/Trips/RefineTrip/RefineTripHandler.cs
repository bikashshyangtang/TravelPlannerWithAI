using TravelPlanner.Application.Abstractions;
using TravelPlanner.Domain.Entities;
using TravelPlanner.Domain.Enums;

namespace TravelPlanner.Application.Trips.RefineTrip;
public class RefineTripHandler
{
    private readonly ITripRepository _tripRepository;
    private readonly IItineraryRefiner _refiner;

    public RefineTripHandler(ITripRepository tripRepository, IItineraryRefiner refiner)
    {
        _tripRepository = tripRepository;
        _refiner = refiner;
    }
    public async Task<RefineTripResponse> HandleAsync(int tripId, string userMessage, CancellationToken cancellationToken)
    {
        var trip  = await _tripRepository.GetByIdAsync(tripId, cancellationToken) ?? throw new InvalidOperationException($"Trip with ID {tripId} not found.");
        trip.ChatMessages.Add(new ChatMessage
        {
            Role = MessageRole.User,
            Content = userMessage
        });
        var assistantReply = await _refiner.RefineAsync(trip, userMessage, cancellationToken);
        trip.ChatMessages.Add(new ChatMessage
        {
            Role = MessageRole.Assistant,
            Content = assistantReply
        });
        await _tripRepository.SaveChangesAsync(cancellationToken);
        return new RefineTripResponse(tripId, assistantReply,
            trip.ChatMessages.OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageResponse(m.Role.ToString(), m.Content, m.CreatedAt))
                .ToList());
    }

    public record RefineTripResponse(
        int TripId,
        string LatestReply,
        List<ChatMessageResponse> Conversation
    );

    public record ChatMessageResponse(
        string Role,
        string Content,
        DateTimeOffset CreatedAt
    );
}