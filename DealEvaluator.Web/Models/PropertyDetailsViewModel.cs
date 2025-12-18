using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Application.DTOs.Evaluation;
using DealEvaluator.Application.DTOs.Lender;
using DealEvaluator.Application.DTOs.Property;

namespace DealEvaluator.Web.Models;

public class PropertyDetailsViewModel
{
    public PropertyDto Property { get; set; }
    public EvaluationDto? LatestEvaluation { get; set; }
    public List<EvaluationDto> EvaluationHistory { get; set; } = new();
    public List<ComparableDto> Comparables { get; set; } = new();
    public List<LenderDto> Lenders { get; set; } = new();
    public int? DefaultLenderId { get; set; }
}
