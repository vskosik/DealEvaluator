using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.DTOs.Rehab;

public class RehabLineItemDto
{
    public int Id { get; set; }
    public int RehabEstimateId { get; set; }
    public RehabLineItemType LineItemType { get; set; }
    public RehabCondition Condition { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
    public decimal EstimatedCost { get; set; }
}
