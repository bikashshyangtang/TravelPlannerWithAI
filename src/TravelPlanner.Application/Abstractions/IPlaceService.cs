namespace TravelPlanner.Application.Abstractions
{
    public interface IPlaceService
    {
        Task<List<PlaceResult>> SearchAsync(string placeName, string interestQuery, CancellationToken cancellationToken);
    }
}