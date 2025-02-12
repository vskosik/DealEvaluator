using Deal_Evaluator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Deal_Evaluator.Data;

public class DealEvaluatorContext : IdentityDbContext<User>
{
    public DealEvaluatorContext(DbContextOptions<DealEvaluatorContext> options) : base(options) { }
    
    public DbSet<Property> Properties { get; set; }
    public DbSet<Comparable> Comparables { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<MarketData> MarketData { get; set; }
    public DbSet<ApiLog> ApiLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>();
        
        builder.Entity<ApiLog>()
            .HasOne(l => l.User)
            .WithMany(l => l.ApiLogs)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Entity<ApiLog>()
            .HasOne(p => p.Property)
            .WithMany(p => p.ApiLogs)
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}