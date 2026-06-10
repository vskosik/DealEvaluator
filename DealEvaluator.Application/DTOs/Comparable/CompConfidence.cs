namespace DealEvaluator.Application.DTOs.Comparable;

/// <summary>
/// Confidence grade for a comparable search result. Lets callers decide how to
/// treat degraded results instead of failing hard.
/// </summary>
public enum CompConfidence
{
    High,         // 3+ comps, strong similarity scores, no missing data
    Medium,       // 3+ comps but weaker similarity or missing data
    Low,          // 1-2 comps, or comps with significant missing data
    Insufficient  // 0 comps after gating — caller should widen geography or bail
}
