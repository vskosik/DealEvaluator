using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.DTOs.Rehab;

public class RehabCostTemplateDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public RehabLineItemType LineItemType { get; set; }
    public RehabCondition Condition { get; set; }
    public decimal DefaultCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
