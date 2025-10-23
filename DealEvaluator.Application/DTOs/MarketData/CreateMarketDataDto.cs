namespace DealEvaluator.Application.DTOs.MarketData;

public class CreateMarketDataDto
{
    public int PropertyId { get; set; }
    public string Source { get; set; }
    public string DataJson { get; set; }
}