public record GeneratedItinerary(
    List<GeneratedDay> Days
);

public record GeneratedDay(
    int DayNumber,
    DateOnly Date,
    string Summary,
    List<GeneratedActivity> Activities
);

public record GeneratedActivity(
    string Name,
    string? Description,
    TimeOnly? StartTime,
    int? DurationMinutes,
    string? Location,
    decimal? EstimatedCost
);
