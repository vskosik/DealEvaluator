using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Domain.Entities;

public class Property : RealEstateEntity
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public PropertyTypes PropertyType { get; set; }
    public PropertyConditions PropertyConditions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}