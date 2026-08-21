using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure.Persistence.Configurations;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Destination).IsRequired().HasMaxLength(200);
        builder.Property(t => t.BudgetLevel).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Budget).HasPrecision(18,2);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasMany(t => t.ItineraryDays)
               .WithOne(d => d.Trip)
               .HasForeignKey(d => d.TripId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.ChatMessages)
               .WithOne(m => m.Trip)
               .HasForeignKey(m => m.TripId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}