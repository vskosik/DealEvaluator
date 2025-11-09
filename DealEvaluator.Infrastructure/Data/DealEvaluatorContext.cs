using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Data;

public class DealEvaluatorContext : IdentityDbContext<User>
{
    public DealEvaluatorContext(DbContextOptions<DealEvaluatorContext> options) : base(options) { }
    
    public DbSet<Property> Properties { get; set; }
    public DbSet<Comparable> Comparables { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<MarketData> MarketData { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new PropertyConfiguration());
        builder.ApplyConfiguration(new MarketDataConfiguration());
        builder.ApplyConfiguration(new ComparableConfiguration());
        builder.ApplyConfiguration(new EvaluationConfiguration());
    }
}