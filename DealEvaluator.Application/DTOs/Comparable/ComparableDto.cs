using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.DTOs.Comparable;

public class ComparableDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public DateTime? SaleDate { get; set; }
    public ListingStatuses ListingStatus { get; set; }
    public string Source { get; set; }

    // From RealEstateEntity
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
}