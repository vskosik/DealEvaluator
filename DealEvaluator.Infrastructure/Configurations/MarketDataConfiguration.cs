using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class MarketDataConfiguration : IEntityTypeConfiguration<MarketData>
{
    public void Configure(EntityTypeBuilder<MarketData> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Source)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(x => x.DataJson).IsRequired();
        
        builder.Property(x => x.LastUpdated)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasOne<Property>()
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}