using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Domain.Entities;

public class DealSettings
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Selling Costs (applied to ARV)
    public double SellingAgentCommission { get; set; } = 0.06;  // 6%
    public double SellingClosingCosts { get; set; } = 0.02;     // 2%

    // Buying Costs (applied to purchase price)
    public double BuyingClosingCosts { get; set; } = 0.02;      // 2%

    // Holding Costs
    public double AnnualPropertyTaxRate { get; set; } = 0.012;  // 1.2%
    public int MonthlyInsurance { get; set; } = 150;         // $150/month
    public int MonthlyUtilities { get; set; } = 200;         // $200/month
    public int DefaultHoldingMonths { get; set; } = 4;            // 4 months
    public double DownPaymentPercentage { get; set; } = 0.2;    // 20%
    public double DefaultLoanRate { get; set; } = 0.12;     // 12%
    public int? DefaultLenderId { get; set; }

    // Profit & Risk Settings
    public ProfitTargetType ProfitTargetType { get; set; } = ProfitTargetType.PercentageOfArv;
    public double ProfitTargetValue { get; set; } = 0.15;       // 15% of ARV or $15,000
    public double ContingencyPercentage { get; set; } = 0.10;   // 10% of rehab costs

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Lender? DefaultLender { get; set; }
}