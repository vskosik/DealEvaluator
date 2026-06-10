namespace DealEvaluator.Application.DTOs.Comparable;

/// <summary>
/// Result of a comparable search, including quality metadata.
/// </summary>
public class CompSearchResult
{
    public List<ScoredComparable> Comparables { get; init; } = new();
    public CompConfidence Confidence { get; init; }
    public int CandidatePoolSize { get; init; }
    public int OutliersRemoved { get; init; }
    public string? Notes { get; init; }
}