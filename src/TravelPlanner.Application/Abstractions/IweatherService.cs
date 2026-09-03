namespace TravelPlanner.Application.Abstractions
{
    public interface IWeatherService
    {
        Task<string> GetWeatherForecastAsync(string location, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    }
}