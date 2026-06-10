using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class CachedPropertyConfiguration : IEntityTypeConfiguration<CachedProperty>
{
    public void Configure(EntityTypeBuilder<CachedProperty> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Zpid)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PropertyType).HasMaxLength(50);
        builder.Property(x => x.Address).HasMaxLength(255);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(50);
        builder.Property(x => x.ZipCode).HasMaxLength(10);
        builder.Property(x => x.DetailUrl).HasMaxLength(500);
        builder.Property(x => x.ListingStatus).HasMaxLength(50);
        builder.Property(x => x.HomeStatus).HasMaxLength(50);

        builder.Ignore(x => x.DateSold);

        builder.HasIndex(x => x.MarketDataId);
        builder.HasIndex(x => new { x.MarketDataId, x.Zpid }).IsUnique();

        builder.HasOne<MarketData>()
            .WithMany(x => x.Properties)
            .HasForeignKey(x => x.MarketDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
