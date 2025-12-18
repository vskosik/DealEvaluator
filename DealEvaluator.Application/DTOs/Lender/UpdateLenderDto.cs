namespace DealEvaluator.Application.DTOs.Lender;

public class UpdateLenderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double AnnualRate { get; set; }
    public double OriginationFee { get; set; }
    public double LoanServiceFee { get; set; }
    public string? Note { get; set; }
}
