namespace DealEvaluator.Domain.Entities;

public abstract class RealEstateEntity
{
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public int? Price { get; set; }
    public int? Sqft { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? LotSizeSqft { get; set; }
    public int? YearBuilt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}