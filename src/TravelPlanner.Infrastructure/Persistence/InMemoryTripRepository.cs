using TravelPlanner.Application.Abstractions;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure.Persistence;

public class InMemoryTripRepository : ITripRepository
{
    private static readonly Dictionary<int, Trip> _trips = new();
    private static int _nextId = 1;
    public Task<Trip?> GetByIdAsync(int tripId, CancellationToken cancellationToken)
    {
        _trips.TryGetValue(tripId, out var trip);
        return Task.FromResult(trip);
    }

    public Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        trip.Id = _nextId++;
        typeof(Trip).GetProperty("Id")!.SetValue(trip, trip.Id);
        _trips[trip.Id] = trip;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<List<Trip>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_trips.Values.ToList());
    }
}