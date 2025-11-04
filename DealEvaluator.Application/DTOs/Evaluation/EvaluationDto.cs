using DealEvaluator.Application.DTOs.Comparable;

namespace DealEvaluator.Application.DTOs.Evaluation;

public class EvaluationDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int? Arv { get; set; }
    public int? RepairCost { get; set; }
    public int? RentalIncome { get; set; }
    public decimal? CapRate { get; set; }
    public decimal? CashOnCash { get; set; }

    // Fix & Flip Calculations
    public int? MaxOffer { get; set; }
    public int? Profit { get; set; }
    public decimal? Roi { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ComparableDto> Comparables { get; set; } = new();
}