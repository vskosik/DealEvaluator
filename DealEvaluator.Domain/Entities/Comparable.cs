using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Domain.Entities;

public class Comparable : RealEstateEntity
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public DateTime? SaleDate { get; set; }
    public ListingStatuses ListingStatus { get; set; }
    public string Source { get; set; }
    public string ListingUrl { get; set; }
    public ComparableType ComparableType { get; set; }
}