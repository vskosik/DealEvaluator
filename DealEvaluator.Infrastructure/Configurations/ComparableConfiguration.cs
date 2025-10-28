using System.Net.NetworkInformation;
using DealEvaluator.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class ComparableConfiguration : IEntityTypeConfiguration<Comparable>
{
    public void Configure(EntityTypeBuilder<Comparable> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Address).IsRequired();
        builder.Property(x => x.City).IsRequired();
        builder.Property(x => x.State).IsRequired();
        builder.Property(x => x.ZipCode).IsRequired();

        builder.Property(x => x.Price).IsRequired(false);
        builder.Property(x => x.Sqft).IsRequired(false);
        builder.Property(x => x.Bedrooms).IsRequired(false);
        builder.Property(x => x.Bathrooms).IsRequired(false);
        builder.Property(x => x.LotSizeSqft).IsRequired(false);
        builder.Property(x => x.YearBuilt).IsRequired(false);

        builder.Property(x => x.SaleDate)
            .IsRequired(false)
            .HasColumnType("datetime");
        
        builder.Property(x => x.ListingStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Source).IsRequired();
    }
}