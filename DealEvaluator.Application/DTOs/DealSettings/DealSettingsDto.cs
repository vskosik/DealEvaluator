using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.DTOs.DealSettings;

public class DealSettingsDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Selling Costs (applied to ARV)
    public double SellingAgentCommission { get; set; }
    public double SellingClosingCosts { get; set; }

    // Buying Costs (applied to purchase price)
    public double BuyingClosingCosts { get; set; }

    // Holding Costs
    public double AnnualPropertyTaxRate { get; set; }
    public int MonthlyInsurance { get; set; }
    public int MonthlyUtilities { get; set; }
    public int DefaultHoldingMonths { get; set; }
    public double DownPaymentPercentage { get; set; }
    public double DefaultLoanRate { get; set; }
    public int? DefaultLenderId { get; set; }

    // Profit & Risk Settings
    public ProfitTargetType ProfitTargetType { get; set; }
    public double ProfitTargetValue { get; set; }
    public double ContingencyPercentage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
