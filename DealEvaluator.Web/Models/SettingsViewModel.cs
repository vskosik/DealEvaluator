using DealEvaluator.Application.DTOs.DealSettings;
using DealEvaluator.Application.DTOs.Rehab;

namespace DealEvaluator.Web.Models;

public class SettingsViewModel
{
    public UpdateDealSettingsDto DealSettings { get; set; } = new();
    public List<RehabCostTemplateDto> RehabTemplates { get; set; } = new();
}