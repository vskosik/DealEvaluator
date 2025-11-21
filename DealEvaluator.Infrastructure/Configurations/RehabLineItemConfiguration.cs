using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class RehabLineItemConfiguration : IEntityTypeConfiguration<RehabLineItem>
{
    public void Configure(EntityTypeBuilder<RehabLineItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RehabEstimateId)
            .IsRequired();

        builder.Property(x => x.LineItemType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Condition)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.UnitCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
            .IsRequired(false)
            .HasMaxLength(500);

        // Ignore computed property
        builder.Ignore(x => x.EstimatedCost);
    }
}