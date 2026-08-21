using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelPlanner.Domain.Entities;

namespace TravelPlanner.Infrastructure.Persistence.Configurations;
public class ActivityConfifuration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(2000);
        builder.Property(d => d.Location).HasMaxLength(300);
        builder.Property(d => d.EstimatedCost).HasPrecision(18, 2);
    }
}