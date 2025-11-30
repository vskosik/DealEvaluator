namespace DealEvaluator.Domain.Entities;

public class MarketData
{
    public int Id { get; set; }

    public string ZipCode { get; set; }

    public string HomeType { get; set; }

    public string Source { get; set; }

    public string RawJson { get; set; }

    public string? Keywords { get; set; }

    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}