using DealEvaluator.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace DealEvaluator.Application.DTOs.DealSettings;

public class UpdateDealSettingsDto
{
    // Selling Costs (applied to ARV)
    [Required]
    [Range(0, 1, ErrorMessage = "Selling agent commission must be between 0 and 100%")]
    public decimal SellingAgentCommission { get; set; }

    [Required]
    [Range(0, 1, ErrorMessage = "Selling closing costs must be between 0 and 100%")]
    public decimal SellingClosingCosts { get; set; }

    // Buying Costs (applied to purchase price)
    [Required]
    [Range(0, 1, ErrorMessage = "Buying closing costs must be between 0 and 100%")]
    public decimal BuyingClosingCosts { get; set; }

    // Holding Costs
    [Required]
    [Range(0, 0.1, ErrorMessage = "Property tax rate must be between 0 and 10%")]
    public decimal AnnualPropertyTaxRate { get; set; }

    [Required]
    [Range(0, 10000, ErrorMessage = "Monthly insurance must be between $0 and $10,000")]
    public decimal MonthlyInsurance { get; set; }

    [Required]
    [Range(0, 10000, ErrorMessage = "Monthly utilities must be between $0 and $10,000")]
    public decimal MonthlyUtilities { get; set; }

    [Required]
    [Range(1, 48, ErrorMessage = "Holding period must be between 1 and 48 months")]
    public int DefaultHoldingMonths { get; set; }

    // Profit & Risk Settings
    [Required]
    public ProfitTargetType ProfitTargetType { get; set; }

    [Required]
    [Range(0, 1000000, ErrorMessage = "Profit target value must be positive")]
    public decimal ProfitTargetValue { get; set; }

    [Required]
    [Range(0, 0.5, ErrorMessage = "Contingency percentage must be between 0 and 50%")]
    public decimal ContingencyPercentage { get; set; }
}