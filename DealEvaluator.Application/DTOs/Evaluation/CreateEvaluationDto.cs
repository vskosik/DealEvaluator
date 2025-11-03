namespace DealEvaluator.Application.DTOs.Evaluation;

public class CreateEvaluationDto
{
    public int PropertyId { get; set; }
    public int? RepairCost { get; set; }
    public int? PurchasePrice { get; set; }
    public List<int> ComparableIds { get; set; } = new(); // IDs of comparables to use for ARV calculation
}