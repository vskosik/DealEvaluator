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
            var dealSettingsDto = new UpdateDealSettingsDto
            {
                SellingAgentCommission = settings.SellingAgentCommission,
                SellingClosingCosts = settings.SellingClosingCosts,
                BuyingClosingCosts = settings.BuyingClosingCosts,
                AnnualPropertyTaxRate = settings.AnnualPropertyTaxRate,
                MonthlyInsurance = settings.MonthlyInsurance,
                MonthlyUtilities = settings.MonthlyUtilities,
                DefaultHoldingMonths = settings.DefaultHoldingMonths,
                ProfitTargetType = settings.ProfitTargetType,
                ProfitTargetValue = settings.ProfitTargetValue,
                ContingencyPercentage = settings.ContingencyPercentage
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
            // Map DTO to entity for update
            var settings = new DealSettings
            {
                UserId = userId,
                SellingAgentCommission = dto.SellingAgentCommission,
                SellingClosingCosts = dto.SellingClosingCosts,
                BuyingClosingCosts = dto.BuyingClosingCosts,
                AnnualPropertyTaxRate = dto.AnnualPropertyTaxRate,
                MonthlyInsurance = dto.MonthlyInsurance,
                MonthlyUtilities = dto.MonthlyUtilities,
                DefaultHoldingMonths = dto.DefaultHoldingMonths,
                ProfitTargetType = dto.ProfitTargetType,
                ProfitTargetValue = dto.ProfitTargetValue,
                ContingencyPercentage = dto.ContingencyPercentage
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