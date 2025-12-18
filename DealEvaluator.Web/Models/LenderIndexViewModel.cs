using DealEvaluator.Application.DTOs.Lender;

namespace DealEvaluator.Web.Models;

public class LenderIndexViewModel
{
    public List<LenderDto> Lenders { get; set; } = new();
    public List<LenderDto> ArchivedLenders { get; set; } = new();
    public int? DefaultLenderId { get; set; }

    public string? DefaultLenderName =>
        DefaultLenderId.HasValue
            ? Lenders.FirstOrDefault(l => l.Id == DefaultLenderId.Value)?.Name
            : null;
}
