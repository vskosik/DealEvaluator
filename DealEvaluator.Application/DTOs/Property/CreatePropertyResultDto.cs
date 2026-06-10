using DealEvaluator.Application.DTOs.Comparable;

namespace DealEvaluator.Application.DTOs.Property;

/// <summary>
/// Result of creating a property, including the outcome of the automatic
/// comp search + evaluation so the UI can surface warnings.
/// </summary>
public class CreatePropertyResultDto
{
    public required PropertyDto Property { get; init; }

    /// <summary>True when an automatic evaluation was created alongside the property.</summary>
    public bool EvaluationCreated { get; init; }

    /// <summary>Confidence of the automatic comp search; null when no search ran.</summary>
    public CompConfidence? CompConfidence { get; init; }

    /// <summary>Diagnostics from the comp search (e.g. why no comps were found).</summary>
    public string? CompSearchNotes { get; init; }
}