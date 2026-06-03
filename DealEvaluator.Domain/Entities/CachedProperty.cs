using System.ComponentModel.DataAnnotations.Schema;

namespace DealEvaluator.Domain.Entities;

public class CachedProperty
{
    public int Id { get; set; }
    public int MarketDataId { get; set; }

    public string Zpid { get; set; }
    public string? PropertyType { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Bedrooms { get; set; }
    public float? Bathrooms { get; set; }
    public int? LivingArea { get; set; }
    public string? DetailUrl { get; set; }
    public string? ListingStatus { get; set; }
    public int? Zestimate { get; set; }
    public int? DaysOnZillow { get; set; }
    public int? Price { get; set; }
    public long? DateSoldTimestamp { get; set; }

    [NotMapped]
    public DateTime? DateSold => DateSoldTimestamp.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(DateSoldTimestamp.Value).DateTime
        : null;
}
