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

    // Fix & Flip Calculations
    public int? MaxOffer { get; set; }
    public int? Profit { get; set; }
    public decimal? Roi { get; set; }
    
    // Selling Costs
    public int? AgentCommission { get; set; }
    public int? SellingClosingCosts { get; set; }

    // Buying Costs
    public int? BuyingClosingCosts { get; set; }

    // Holding Costs
    public int? PropertyTaxesCost { get; set; }
    public int? InsuranceCost { get; set; }
    public int? UtilitiesCost { get; set; }

    // Financing Costs
    public int? DownPayment { get; set; }
    public int? LoanAmount { get; set; }
    public int? MonthlyPayment { get; set; }
    public int? TotalInterest { get; set; }
    public int? OriginationFeeCost { get; set; }
    public int? LoanServiceFeeCost { get; set; }
    public int? TotalFinancingCosts { get; set; }

    public int? ContingencyBuffer { get; set; }
    public int? LenderId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Comparable> Comparables { get; set; } = new List<Comparable>();
    public RehabEstimate? RehabEstimate { get; set; }
    public Lender? Lender { get; set; }

    // Computed - Ignored
    public int? RepairCost => RehabEstimate != null ? (int?)Math.Round(RehabEstimate.TotalCost) : null;
}
