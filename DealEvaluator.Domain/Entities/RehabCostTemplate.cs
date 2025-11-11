using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Domain.Entities;

public class RehabCostTemplate
{
    public int Id { get; set; }
    public string UserId { get; set; }

    public RehabLineItemType LineItemType { get; set; }
    public RehabCondition Condition { get; set; }
    public decimal DefaultCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; }
}