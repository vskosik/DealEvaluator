using System.ComponentModel.DataAnnotations.Schema;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Domain.Entities;

public class RehabLineItem
{
    public int Id { get; set; }
    public int RehabEstimateId { get; set; }

    public RehabLineItemType LineItemType { get; set; }
    public RehabCondition Condition { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }

    public string? Notes { get; set; }

    public RehabEstimate RehabEstimate { get; set; }
    
    // Computed - Ignored
    public decimal EstimatedCost => Quantity * UnitCost;
}