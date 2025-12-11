using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.DTOs.DealSettings;

public class DealSettingsDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Selling Costs (applied to ARV)
    public decimal SellingAgentCommission { get; set; }
    public decimal SellingClosingCosts { get; set; }

    // Buying Costs (applied to purchase price)
    public decimal BuyingClosingCosts { get; set; }

    // Holding Costs
    public decimal AnnualPropertyTaxRate { get; set; }
    public decimal MonthlyInsurance { get; set; }
    public decimal MonthlyUtilities { get; set; }
    public int DefaultHoldingMonths { get; set; }

    // Profit & Risk Settings
    public ProfitTargetType ProfitTargetType { get; set; }
    public decimal ProfitTargetValue { get; set; }
    public decimal ContingencyPercentage { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}