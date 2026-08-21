using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure.Persistence.Configurations;
public class ItineraryDayConfiguration : IEntityTypeConfiguration<ItineraryDay>
{
    public void Configure(EntityTypeBuilder<ItineraryDay> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Summary).HasMaxLength(1000);
        builder.HasMany(d => d.Activities)
               .WithOne(a => a.ItineraryDay)
               .HasForeignKey(a => a.ItineraryDayId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}