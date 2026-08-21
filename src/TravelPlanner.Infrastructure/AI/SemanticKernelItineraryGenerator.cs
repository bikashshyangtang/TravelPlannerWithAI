using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Reflection;
using System.Text.Json;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Application.Trips;


namespace TravelPlanner.Infrastructure.AI;
public class SemanticKernelItineraryGenerator : IItineraryGenerator
{
    private readonly Kernel _kernel;
    private readonly string _systemPrompt;
    private readonly IChatCompletionService _chatService;


    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public SemanticKernelItineraryGenerator(Kernel kernel)
    {
        _kernel = kernel;
        _chatService = _kernel.GetRequiredService<IChatCompletionService>(); // Get the chat completion service from the kernel
        _systemPrompt = LoadPrompt();
    }

    public async Task<GeneratedItinerary> GenerateAsync(TripCriteria criteria, CancellationToken cancellationToken)
    {
       var userMessage = BuildUserMessage(criteria);
       var history = new ChatHistory();
       history.AddSystemMessage(_systemPrompt);
       history.AddUserMessage(userMessage); 
       var settings = new PromptExecutionSettings
       {
         FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()  
       };
       var response = await _chatService.GetChatMessageContentAsync(history, settings, _kernel, cancellationToken);
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
}