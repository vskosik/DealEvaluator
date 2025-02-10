using Deal_Evaluator.Models;
using Microsoft.EntityFrameworkCore;

namespace Deal_Evaluator.Data;

public class DealEvaluatorContext : DbContext
{
    public DealEvaluatorContext(DbContextOptions<DealEvaluatorContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Comparable> Comparables { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<MarketData> MarketData { get; set; }
    public DbSet<ApiLog> ApiLogs { get; set; }
}