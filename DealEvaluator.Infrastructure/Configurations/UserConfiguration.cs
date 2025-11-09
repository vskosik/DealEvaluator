using DealEvaluator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DealEvaluator.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.CompanyName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(u => u.ApiCallCount)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(u => u.ApiCallCountResetDate)
            .IsRequired(false);

        builder.HasMany<Property>()
            .WithOne()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}