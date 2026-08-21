using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Application.Abstractions;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(int tripId, CancellationToken cancellationToken);
    Task<List<Trip>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Trip trip, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}