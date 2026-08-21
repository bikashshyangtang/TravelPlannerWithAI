using Microsoft.EntityFrameworkCore;
using TravelPlanner.Application.Abstractions;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure.Persistence;

public class TripRepository : ITripRepository
{
    private readonly TravelPlannerDbContext _dbContext;

    public TripRepository(TravelPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Trip?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Trips
            .Include(t => t.ItineraryDays)
                .ThenInclude(d => d.Activities)
            .Include(t => t.ChatMessages)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        await _dbContext.Trips.AddAsync(trip, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    public async Task<List<Trip>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Trips
            .AsNoTracking()
            .OrderByDescending(t => t.Id)
            .Include(t => t.ItineraryDays)
                .ThenInclude(d => d.Activities)
            .ToListAsync(cancellationToken);
    }
}