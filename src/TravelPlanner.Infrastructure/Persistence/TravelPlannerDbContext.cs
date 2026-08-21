using Microsoft.EntityFrameworkCore;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure.Persistence;

public class TravelPlannerDbContext : DbContext
{
    public TravelPlannerDbContext(DbContextOptions<TravelPlannerDbContext> options) : base(options)
    {

    }

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<ItineraryDay> ItineraryDays => Set<ItineraryDay>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravelPlannerDbContext).Assembly);
    }
}
