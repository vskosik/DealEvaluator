namespace DealEvaluator.Application.DTOs.MarketData;

public class MarketDataDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string Source { get; set; }
    public string DataJson { get; set; }
    public DateTime LastUpdated { get; set; }
}