using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Domain.Entities;

public class DealSettings
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Selling Costs (applied to ARV)
    public decimal SellingAgentCommission { get; set; } = 0.06m;  // 6%
    public decimal SellingClosingCosts { get; set; } = 0.02m;     // 2%

    // Buying Costs (applied to purchase price)
    public decimal BuyingClosingCosts { get; set; } = 0.02m;      // 2%

    // Holding Costs
    public decimal AnnualPropertyTaxRate { get; set; } = 0.012m;  // 1.2%
    public decimal MonthlyInsurance { get; set; } = 150m;         // $150/month
    public decimal MonthlyUtilities { get; set; } = 200m;         // $200/month
    public int DefaultHoldingMonths { get; set; } = 4;            // 4 months

    // Profit & Risk Settings
    public ProfitTargetType ProfitTargetType { get; set; } = ProfitTargetType.PercentageOfArv;
    public decimal ProfitTargetValue { get; set; } = 0.15m;       // 15% of ARV or $15,000
    public decimal ContingencyPercentage { get; set; } = 0.10m;   // 10% of rehab costs

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}