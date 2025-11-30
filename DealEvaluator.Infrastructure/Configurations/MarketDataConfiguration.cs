using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class MarketDataConfiguration : IEntityTypeConfiguration<MarketData>
{
    public void Configure(EntityTypeBuilder<MarketData> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ZipCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.HomeType)
            .IsRequired()
            .HasMaxLength(50);

        // Create index on ZipCode for fast lookups
        builder.HasIndex(x => x.ZipCode);

        builder.Property(x => x.Source)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RawJson)
            .IsRequired();

        builder.Property(x => x.Keywords)
            .IsRequired(false)
            .HasDefaultValue("");

        builder.HasIndex(x => new { x.ZipCode, x.HomeType, x.Keywords }).IsUnique();

        builder.Property(x => x.FetchedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.ExpiresAt)
            .IsRequired(false);
    }
}