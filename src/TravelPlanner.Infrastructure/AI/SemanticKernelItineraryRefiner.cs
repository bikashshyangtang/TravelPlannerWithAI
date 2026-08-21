using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Domain.Entities;
using TravelPlanner.Domain.Enums;

namespace TravelPlanner.Infrastructure.AI;

public class SemanticKernelItineraryRefiner : IItineraryRefiner
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public SemanticKernelItineraryRefiner(Kernel kernel)
    {
        _kernel = kernel;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> RefineAsync(Trip trip, string userMessage, CancellationToken cancellationToken)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage($"You are an expert travel planner helping refine a trip itinerary."

            +$"\nCurrent trip: {trip.Destination}"
            +$"\nDates: {trip.StartDate:yyyy-MM-dd} to {trip.EndDate:yyyy-MM-dd}"
            +$"\nBudget: ${trip.Budget} ({trip.BudgetLevel})"
            +$"\nDuration: {trip.DurationInDays} days"
            
            +"\nCurrent itinerary:"
            +$"\n{BuildItinerarySummary(trip)}"

            +"\nHelp the user refine their trip. Be specific and practical."
            +"\nIf they ask to change activities, suggest specific alternatives."
            +"\nIf they ask about budget, calculate realistic costs.");

        foreach (var message in trip.ChatMessages.OrderBy(m => m.CreatedAt))
        {
            if (message.Role == MessageRole.User)
            {
                chatHistory.AddUserMessage(message.Content);
            }
            else if (message.Role == MessageRole.Assistant)
            {
                chatHistory.AddAssistantMessage(message.Content);
            }
        }
        var response = await _chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            kernel: _kernel,
            cancellationToken: cancellationToken
        );
        return response.Content ?? throw new InvalidOperationException("AI returned empty refinement response.");
    }

    private static string BuildItinerarySummary(Trip trip)
    {
        var lines = new List<string>();

        foreach (var day in trip.ItineraryDays.OrderBy(d => d.DayNumber))
        {
            lines.Add($"Day {day.DayNumber} ({day.Date:yyyy-MM-dd}): {day.Summary}");
            foreach (var activity in day.Activities.OrderBy(a => a.StartTime))
            {
                var cost = activity.EstimatedCost.HasValue
                    ? $"${activity.EstimatedCost:F2}"
                    : "cost unknown";
                lines.Add($"  - {activity.Name} at {activity.StartTime:HH:mm} ({cost})");
            }
        }

        return string.Join("\n", lines);
    }
}