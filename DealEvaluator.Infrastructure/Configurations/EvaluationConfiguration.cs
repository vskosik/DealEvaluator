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
        
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}