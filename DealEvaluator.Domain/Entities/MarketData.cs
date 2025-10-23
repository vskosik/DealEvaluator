namespace DealEvaluator.Domain.Entities;

public class MarketData
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string Source { get; set; }
    public string DataJson { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}