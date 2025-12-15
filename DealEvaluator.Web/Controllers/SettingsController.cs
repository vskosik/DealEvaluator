using System.Security.Claims;
using DealEvaluator.Application.DTOs.DealSettings;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealEvaluator.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly IDealSettingsService _dealSettingsService;
    private readonly IRehabCostTemplateService _rehabTemplateService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        IDealSettingsService dealSettingsService,
        IRehabCostTemplateService rehabTemplateService,
        ILogger<SettingsController> logger)
    {
        _dealSettingsService = dealSettingsService;
        _rehabTemplateService = rehabTemplateService;
        _logger = logger;
    }

    // GET: Settings/Index - Display unified settings page with deal settings and rehab templates
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Load deal settings
            var settings = await _dealSettingsService.GetUserSettingsAsync(userId);

            // Convert decimals to percentages for display
            var dealSettingsDto = new UpdateDealSettingsDto
            {
                SellingAgentCommission = Math.Round(settings.SellingAgentCommission * 100, 1),
                SellingClosingCosts = Math.Round(settings.SellingClosingCosts * 100, 1),
                BuyingClosingCosts = Math.Round(settings.BuyingClosingCosts * 100, 1),
                AnnualPropertyTaxRate = Math.Round(settings.AnnualPropertyTaxRate * 100, 1),
                MonthlyInsurance = settings.MonthlyInsurance,
                MonthlyUtilities = settings.MonthlyUtilities,
                DefaultHoldingMonths = settings.DefaultHoldingMonths,
                DownPaymentPercentage = Math.Round(settings.DownPaymentPercentage * 100, 1),
                DefaultLoanRate = Math.Round(settings.DefaultLoanRate * 100, 1),
                ProfitTargetType = settings.ProfitTargetType,
                ProfitTargetValue = settings.ProfitTargetType == Domain.Enums.ProfitTargetType.PercentageOfArv
                    ? Math.Round(settings.ProfitTargetValue * 100, 1)
                    : settings.ProfitTargetValue,
                ContingencyPercentage = Math.Round(settings.ContingencyPercentage * 100, 1)
            };

            // Load rehab templates
            var rehabTemplates = await _rehabTemplateService.GetUserTemplatesAsync(userId);

            // Create unified view model
            var viewModel = new SettingsViewModel
            {
                DealSettings = dealSettingsDto,
                RehabTemplates = rehabTemplates.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error loading your settings.";
            return View(new SettingsViewModel());
        }
    }

    // POST: Settings/Index - Update user's deal settings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UpdateDealSettingsDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Please correct the errors in the form.";

            // Reload rehab templates to return full view model
            var rehabTemplates = await _rehabTemplateService.GetUserTemplatesAsync(userId);
            var viewModel = new SettingsViewModel
            {
                DealSettings = dto,
                RehabTemplates = rehabTemplates.ToList()
            };
            return View(viewModel);
        }

        try
        {
            // Convert percentages back to decimals for storage (divide by 100)
            var settings = new DealSettings
            {
                UserId = userId,
                SellingAgentCommission = dto.SellingAgentCommission / 100,
                SellingClosingCosts = dto.SellingClosingCosts / 100,
                BuyingClosingCosts = dto.BuyingClosingCosts / 100,
                AnnualPropertyTaxRate = dto.AnnualPropertyTaxRate / 100,
                MonthlyInsurance = dto.MonthlyInsurance,
                MonthlyUtilities = dto.MonthlyUtilities,
                DefaultHoldingMonths = dto.DefaultHoldingMonths,
                DownPaymentPercentage = dto.DownPaymentPercentage / 100,
                DefaultLoanRate = dto.DefaultLoanRate / 100,
                ProfitTargetType = dto.ProfitTargetType,
                ProfitTargetValue = dto.ProfitTargetType == Domain.Enums.ProfitTargetType.PercentageOfArv
                    ? dto.ProfitTargetValue / 100
                    : dto.ProfitTargetValue,
                ContingencyPercentage = dto.ContingencyPercentage / 100
            };

            await _dealSettingsService.UpdateSettingsAsync(userId, settings);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Settings saved successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deal settings");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "An error occurred while saving your settings.";

            // Reload rehab templates to return full view model
            var rehabTemplates = await _rehabTemplateService.GetUserTemplatesAsync(userId);
            var viewModel = new SettingsViewModel
            {
                DealSettings = dto,
                RehabTemplates = rehabTemplates.ToList()
            };
            return View(viewModel);
        }
    }

    // POST: Settings/ResetToDefaults - Reset settings to default values
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetToDefaults()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            await _dealSettingsService.ResetToDefaultsAsync(userId);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Settings reset to defaults successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting deal settings to defaults");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "An error occurred while resetting your settings.";
            return RedirectToAction(nameof(Index));
        }
    }
}