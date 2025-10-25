using DealEvaluator.Application.DTOs.Evaluation;
using DealEvaluator.Application.DTOs.Property;

namespace DealEvaluator.Web.Models;

public class PropertyDetailsViewModel
{
    public PropertyDto Property { get; set; }
    public EvaluationDto? LatestEvaluation { get; set; }
    public List<EvaluationDto> EvaluationHistory { get; set; } = new();
}