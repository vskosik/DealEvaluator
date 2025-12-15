using DealEvaluator.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace DealEvaluator.Application.DTOs.DealSettings;

public class UpdateDealSettingsDto
{

    [Required]
    [Range(0, 100, ErrorMessage = "Selling agent commission must be between 0 and 100%")]
    public double SellingAgentCommission { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Selling closing costs must be between 0 and 100%")]
    public double SellingClosingCosts { get; set; }


    [Required]
    [Range(0, 100, ErrorMessage = "Buying closing costs must be between 0 and 100%")]
    public double BuyingClosingCosts { get; set; }


    [Required]
    [Range(0, 10, ErrorMessage = "Property tax rate must be between 0 and 10%")]
    public double AnnualPropertyTaxRate { get; set; }

    [Required]
    [Range(0, 10000, ErrorMessage = "Monthly insurance must be between $0 and $10,000")]
    public int MonthlyInsurance { get; set; }

    [Required]
    [Range(0, 10000, ErrorMessage = "Monthly utilities must be between $0 and $10,000")]
    public int MonthlyUtilities { get; set; }

    [Required]
    [Range(1, 48, ErrorMessage = "Holding period must be between 1 and 48 months")]
    public int DefaultHoldingMonths { get; set; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "Down payment must be between 0 and 100%")]
    public double DownPaymentPercentage { get; set; }
    
    [Required]
    [Range(0, 100, ErrorMessage = "Loan rate must be between 0 and 100%")]
    public double DefaultLoanRate { get; set; }


    [Required]
    public ProfitTargetType ProfitTargetType { get; set; }

    [Required]
    [Range(0, 1000000, ErrorMessage = "Profit target value must be positive")]
    public double ProfitTargetValue { get; set; }

    [Required]
    [Range(0, 50, ErrorMessage = "Contingency percentage must be between 0 and 50%")]
    public double ContingencyPercentage { get; set; }
}