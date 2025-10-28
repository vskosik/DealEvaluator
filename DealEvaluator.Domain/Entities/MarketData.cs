namespace DealEvaluator.Domain.Entities;

/// <summary>
/// Represents cached market data from external sources (e.g., Zillow) for a specific zip code.
/// This data is used to avoid repeated API calls and contains raw property listings.
/// </summary>
public class MarketData
{
    public int Id { get; set; }

    /// <summary>
    /// Zip code this market data belongs to
    /// </summary>
    public string ZipCode { get; set; }

    /// <summary>
    /// Source of the data (e.g., "Zillow", "Manual")
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Raw JSON response from the API containing all property listings for this zip code
    /// </summary>
    public string RawJson { get; set; }

    /// <summary>
    /// When this data was fetched from the source
    /// </summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this data should be considered stale and refreshed
    /// TODO: Implement cache expiration logic to automatically refresh stale data
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}