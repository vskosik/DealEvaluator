namespace DealEvaluator.Application.DTOs.Evaluation;

public class EvaluationDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int? Arv { get; set; }
    public int? RepairCost { get; set; }
    public int? PurchasePrice { get; set; }
    public int? RentalIncome { get; set; }
    public int? CapRate { get; set; }
    public int? CashOnCash { get; set; }
    public DateTime CreatedAt { get; set; }
}