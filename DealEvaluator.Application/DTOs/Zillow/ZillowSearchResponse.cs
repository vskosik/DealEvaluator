using System.Text.Json.Serialization;

namespace DealEvaluator.Application.DTOs.Zillow;

public class ZillowSearchResponse
{
    [JsonPropertyName("props")] 
    public List<ZillowProperty> Properties { get; set; }
    
    [JsonPropertyName("totalResultCount")]
    public int TotalResults { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}