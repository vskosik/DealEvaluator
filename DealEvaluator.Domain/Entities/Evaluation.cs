using System.ComponentModel.DataAnnotations.Schema;

namespace DealEvaluator.Domain.Entities;

public class Evaluation
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int? Arv { get; set; }
    public int? RentalIncome { get; set; }
    public decimal? CapRate { get; set; }
    public decimal? CashOnCash { get; set; }

    // Fix & Flip Calculations (70% Rule)
    public int? MaxOffer { get; set; }
    public int? Profit { get; set; }
    public decimal? Roi { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Comparable> Comparables { get; set; } = new List<Comparable>();
    public RehabEstimate? RehabEstimate { get; set; }

    // Computed - Ignored
    public int? RepairCost => RehabEstimate != null ? (int?)Math.Round(RehabEstimate.TotalCost) : null;
}