using DealEvaluator.Application.DTOs.Rehab;

namespace DealEvaluator.Application.DTOs.Evaluation;

public class CreateEvaluationDto
{
    public int PropertyId { get; set; }
    public List<int> ComparableIds { get; set; } = new();
    public List<CreateRehabLineItemDto> RehabLineItems { get; set; } = new();
    public int? LenderId { get; set; }
    public bool UseDefaultLoanRate { get; set; }
}
