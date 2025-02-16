using System.Text.Json.Serialization;

namespace Deal_Evaluator.DTOs.Zillow;

public class ZillowProperty
{
    [JsonPropertyName("zpid")]
    public int Id { get; set; }
    public int Bathrooms { get; set; }
    public int Bedrooms { get; set; }
    public string PropertyType { get; set; }
    public string Address { get; set; }
    public string DetailUrl { get; set; }
    public int Zestimate { get; set; }
    public int DaysOnZillow { get; set; }
    public int Price { get; set; }
    public string ListingStatus { get; set; }
    
    [JsonPropertyName("dateSold")]
    public int DateSoldTimestamp { get; set; }

    [JsonIgnore]
    public DateTime DateSold => DateTimeOffset.FromUnixTimeMilliseconds(DateSoldTimestamp).DateTime;
}