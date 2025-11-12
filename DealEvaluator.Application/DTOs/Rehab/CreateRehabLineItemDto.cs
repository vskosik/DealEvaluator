using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.DTOs.Rehab;

public class CreateRehabLineItemDto
{
    public RehabLineItemType LineItemType { get; set; }
    public RehabCondition Condition { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}
