using System.Text.Json.Serialization;

namespace DealEvaluator.Application.DTOs.Zillow;

public class ZillowProperty
{
    [JsonPropertyName("zpid")]
    public string Id { get; set; }
    public float? Bathrooms { get; set; }
    public int? Bedrooms { get; set; }
    public string PropertyType { get; set; }
    public string Address { get; set; }
    public string DetailUrl { get; set; }
    public int? Zestimate { get; set; }
    public int? DaysOnZillow { get; set; }
    public int? Price { get; set; }
    public string ListingStatus { get; set; }
    
    [JsonPropertyName("dateSold")]
    public long? DateSoldTimestamp { get; set; }

    [JsonIgnore]
    public DateTime? DateSold => DateSoldTimestamp.HasValue 
        ? DateTimeOffset.FromUnixTimeMilliseconds(DateSoldTimestamp.Value).DateTime 
        : null;
}