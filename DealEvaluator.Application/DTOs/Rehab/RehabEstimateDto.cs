namespace DealEvaluator.Application.DTOs.Rehab;

public class RehabEstimateDto
{
    public int Id { get; set; }
    public int EvaluationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalCost { get; set; }
    public List<RehabLineItemDto> LineItems { get; set; } = new();
}
