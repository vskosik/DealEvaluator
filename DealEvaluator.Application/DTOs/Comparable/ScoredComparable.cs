using DealEvaluator.Application.DTOs.Zillow;

namespace DealEvaluator.Application.DTOs.Comparable;

/// <summary>
/// A comparable plus its similarity score and per-dimension breakdown,
/// so the UI can explain *why* a comp was chosen.
/// </summary>
public class ScoredComparable
{
    public required ZillowProperty Property { get; init; }
    public decimal Score { get; init; }          // 0..1, higher = more similar
    public decimal? SqftScore { get; init; }
    public decimal? BedScore { get; init; }
    public decimal? BathScore { get; init; }
    public decimal? RecencyScore { get; init; }
    public decimal? DistanceScore { get; init; }
    public decimal? DistanceMiles { get; init; }
    public bool HasMissingData { get; init; }
}