using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class RehabEstimateConfiguration : IEntityTypeConfiguration<RehabEstimate>
{
    public void Configure(EntityTypeBuilder<RehabEstimate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EvaluationId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // One-to-one relationship with Evaluation
        builder.HasOne(x => x.Evaluation)
            .WithOne(x => x.RehabEstimate)
            .HasForeignKey<RehabEstimate>(x => x.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationship with RehabLineItems
        builder.HasMany(x => x.LineItems)
            .WithOne(x => x.RehabEstimate)
            .HasForeignKey(x => x.RehabEstimateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}