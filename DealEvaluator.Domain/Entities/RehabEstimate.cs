using System.ComponentModel.DataAnnotations.Schema;

namespace DealEvaluator.Domain.Entities;

public class RehabEstimate
{
    public int Id { get; set; }
    public int EvaluationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Evaluation Evaluation { get; set; }
    public ICollection<RehabLineItem> LineItems { get; set; } = new List<RehabLineItem>();

    // Computed - Ignored
    public decimal TotalCost => LineItems?.Sum(x => x.EstimatedCost) ?? 0;
}