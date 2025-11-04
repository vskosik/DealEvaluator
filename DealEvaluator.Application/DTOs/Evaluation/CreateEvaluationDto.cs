namespace DealEvaluator.Application.DTOs.Evaluation;

public class CreateEvaluationDto
{
    public int PropertyId { get; set; }
    public int? RepairCost { get; set; }
    public List<int> ComparableIds { get; set; } = new();
}