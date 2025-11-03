using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Arv).IsRequired(false);
        builder.Property(x => x.RepairCost).IsRequired(false);
        builder.Property(x => x.PurchasePrice).IsRequired(false);
        builder.Property(x => x.RentalIncome).IsRequired(false);
        builder.Property(x => x.CapRate).IsRequired(false);
        builder.Property(x => x.CashOnCash).IsRequired(false);

        // Fix & Flip Calculations
        builder.Property(x => x.MaxOffer).IsRequired(false);
        builder.Property(x => x.Profit).IsRequired(false);
        builder.Property(x => x.Roi)
            .IsRequired(false)
            .HasPrecision(5, 2); // e.g., 123.45%

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Many-to-many relationship with Comparables
        builder.HasMany(x => x.Comparables)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "EvaluationComparable",
                j => j.HasOne<Comparable>().WithMany().HasForeignKey("ComparableId").OnDelete(DeleteBehavior.NoAction),
                j => j.HasOne<Evaluation>().WithMany().HasForeignKey("EvaluationId").OnDelete(DeleteBehavior.NoAction)
            );
    }
}