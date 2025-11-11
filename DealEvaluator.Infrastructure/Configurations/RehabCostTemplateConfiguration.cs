using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class RehabCostTemplateConfiguration : IEntityTypeConfiguration<RehabCostTemplate>
{
    public void Configure(EntityTypeBuilder<RehabCostTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.LineItemType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Condition)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.DefaultCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationship with User
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite index to ensure one template per user per line item type per condition
        builder.HasIndex(x => new { x.UserId, x.LineItemType, x.Condition })
            .IsUnique();
    }
}