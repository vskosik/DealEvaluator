using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.ZipCode)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(x => x.Price).IsRequired(false);
        builder.Property(x => x.Sqft).IsRequired(false);
        builder.Property(x => x.Bedrooms).IsRequired(false);
        builder.Property(x => x.Bathrooms).IsRequired(false);
        builder.Property(x => x.LotSizeSqft).IsRequired(false);
        builder.Property(x => x.YearBuilt).IsRequired(false);
        
        builder.Property(x => x.PropertyType)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(x => x.PropertyConditions)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasMany<ApiLog>()
            .WithOne()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany<Evaluation>()
            .WithOne()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany<Comparable>()
            .WithOne()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}