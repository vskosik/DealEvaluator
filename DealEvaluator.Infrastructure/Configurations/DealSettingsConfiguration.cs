using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class DealSettingsConfiguration : IEntityTypeConfiguration<DealSettings>
{
    public void Configure(EntityTypeBuilder<DealSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        // Selling Costs
        builder.Property(x => x.SellingAgentCommission)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(x => x.SellingClosingCosts)
            .IsRequired()
            .HasPrecision(5, 4);

        // Buying Costs
        builder.Property(x => x.BuyingClosingCosts)
            .IsRequired()
            .HasPrecision(5, 4);

        // Holding Costs
        builder.Property(x => x.AnnualPropertyTaxRate)
            .IsRequired()
            .HasPrecision(5, 4);

        builder.Property(x => x.MonthlyInsurance)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.MonthlyUtilities)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(x => x.DefaultHoldingMonths)
            .IsRequired();

        builder.Property(x => x.DefaultLenderId)
            .IsRequired(false);

        // Profit & Risk Settings
        builder.Property(x => x.ProfitTargetType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.ProfitTargetValue)
            .IsRequired()
            .HasPrecision(10, 4);

        builder.Property(x => x.ContingencyPercentage)
            .IsRequired()
            .HasPrecision(5, 4);

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.DefaultLender)
            .WithMany()
            .HasForeignKey(x => x.DefaultLenderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}