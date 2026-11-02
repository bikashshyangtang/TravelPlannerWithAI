using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Reflection;
using System.Text.Json;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Application.Trips;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace TravelPlanner.Infrastructure.AI;
public class SemanticKernelItineraryGenerator : IItineraryGenerator
{
    private readonly Kernel _kernel;
    private readonly string _systemPrompt;
    private readonly IChatCompletionService _chatService;
    private readonly IPlaceService _placeService;


    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public SemanticKernelItineraryGenerator(Kernel kernel, IPlaceService placeService)
    {
        _kernel = kernel;
        _chatService = _kernel.GetRequiredService<IChatCompletionService>(); // Get the chat completion service from the kernel
        _systemPrompt = LoadPrompt();
        _placeService = placeService;
    }


    

    public async Task<GeneratedItinerary> GenerateAsync(TripCriteria criteria, CancellationToken cancellationToken)
    {
        var placesContext = await BuildPlacesContextAsync(criteria, cancellationToken);
        var userMessage = BuildUserMessage(criteria);
        var history = new ChatHistory();
        history.AddSystemMessage(_systemPrompt);
        history.AddSystemMessage(placesContext);
        history.AddUserMessage(userMessage); 
        var settings = new OpenAIPromptExecutionSettings
        {
            //FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            MaxTokens = 6000,
            Temperature = 0.7,
        };
        var response = await _chatService.GetChatMessageContentAsync(history, settings, _kernel, cancellationToken);
        Console.WriteLine($"[AI] Content: '{response.Content}'");
        Console.WriteLine($"[AI] Items: {response.Items?.Count ?? 0}");
        if (response.Items is not null)
            foreach (var item in response.Items)
                Console.WriteLine($"[AI]   {item.GetType().Name}: {item}");
        Console.WriteLine($"[AI] FinishReason: {response.Metadata?["FinishReason"]}");
        Console.WriteLine($"[AI] Usage: {response.Metadata?["Usage"]}");
        var content = response.Content?? throw new InvalidOperationException("The AI returned an empty response.");
        return ParseResponse(content, criteria);
    }

    private static string LoadPrompt()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("PlanTrip.txt", StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException("Embedded resource 'PlanTrip.txt' not found.");
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string BuildUserMessage(TripCriteria criteria)
    {
       var interests = criteria.Interests.Any() ? string.Join(",", criteria.Interests) : "No specific interests";
       return $"Plan a trip with these details:" +
                $"\nDestination: {criteria.Destination}" +
                $"\nStart Date: {criteria.StartDate:yyyy-MM-dd}" +
                $"\nEnd Date: {criteria.EndDate:yyyy-MM-dd}" +
                $"\nDuration: {criteria.EndDate.DayNumber - criteria.StartDate.DayNumber + 1} days" +
                $"\nBudget: {criteria.Budget}" +
                $"\nInterests: {interests}" +
                $"\n Remember: respond with raw JSON only. No markdown, no explanations.";
    } 


    private static GeneratedItinerary ParseResponse(string content, TripCriteria criteria)
    {
        var json = ExtractJson(content);
        try
        {
            var itinerary = JsonSerializer.Deserialize<GeneratedItinerary>(json, _jsonSerializerOptions);
            if (itinerary == null || itinerary.Days.Count == 0)
            {
                return BuildFallBack(criteria, "AI returned empty itinerary");
            }
            return itinerary;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[AI] JSON parsing error: {ex.Message}");
            Console.WriteLine($"[AI] Response content: {content}");
            return BuildFallBack(criteria, "AI returned unparsable response");
        }
    }

    private static string ExtractJson(string content)
    {
        content = content.Trim();
        if (content.StartsWith("'''"))
        {
            var startIndex = content.IndexOf('\n') + 1;
            var endIndex = content.LastIndexOf("'''");
            if (startIndex >= 0 && endIndex > startIndex)
            {
                content = content[startIndex..endIndex].Trim();
            }
        }
        return content;
    }

    private static GeneratedItinerary BuildFallBack(TripCriteria criteria, string reason)
    {
        Console.WriteLine($"[AI] Using fallback: {reason}");
        var days = new List<GeneratedDay>();
        var daysCount = (criteria.EndDate.DayNumber - criteria.StartDate.DayNumber + 1);
        for (int i = 0; i < daysCount; i++)
        {
            days.Add(new GeneratedDay(
                DayNumber: i + 1,
                Date: criteria.StartDate.AddDays(i),
                Summary: $"Day {i + 1} in {criteria.Destination}.",
                Activities: new List<GeneratedActivity>
                {
                    new("Morning exploration", $"Explore {criteria.Destination}", new TimeOnly(9,0), 120, null, null)
                }
            ));
        }
        return new GeneratedItinerary(days);
    }

    private async Task<string> BuildPlacesContextAsync(
    TripCriteria criteria,
    CancellationToken cancellationToken)
    {
        try
        {
            var query = criteria.Interests.Any()
                ? string.Join(", ", criteria.Interests)
                : "tourist attractions";

            var places = await _placeService.SearchAsync(
                criteria.Destination, query, cancellationToken);

            if (places.Count == 0)
                return "No verified venue data available. Use well-known landmarks for this destination.";

            var list = string.Join("\n", places.Select(p =>
                $"- {p.Name} ({p.Category ?? "place"}) — {p.Address ?? "address unknown"}"));

            return $"""
                Real, verified venues at this destination:
                {list}

                Use these actual place names and addresses in the itinerary where they fit.
                """;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Places] Lookup failed: {ex.Message}");
            return "Venue data unavailable. Use well-known landmarks for this destination.";
        }
    }
}