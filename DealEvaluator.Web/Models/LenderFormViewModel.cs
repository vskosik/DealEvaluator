using System.ComponentModel.DataAnnotations;

namespace DealEvaluator.Web.Models;

public class LenderFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, 100, ErrorMessage = "Annual rate must be between 0 and 100%")]
    public double AnnualRate { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Origination fee must be between 0 and 100%")]
    public double OriginationFee { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Loan service fee must be between 0 and 100%")]
    public double LoanServiceFee { get; set; }

    [StringLength(2000)]
    public string? Note { get; set; }
}
