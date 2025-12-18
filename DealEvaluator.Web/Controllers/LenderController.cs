using System.Security.Claims;
using DealEvaluator.Application.DTOs.Lender;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealEvaluator.Web.Controllers;

[Authorize]
public class LenderController : Controller
{
    private readonly ILenderService _lenderService;
    private readonly IDealSettingsService _dealSettingsService;
    private readonly ILogger<LenderController> _logger;

    public LenderController(
        ILenderService lenderService,
        IDealSettingsService dealSettingsService,
        ILogger<LenderController> logger)
    {
        _lenderService = lenderService;
        _dealSettingsService = dealSettingsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var lenders = await _lenderService.GetUserLendersAsync(userId, includeArchived: true);
            var settings = await _dealSettingsService.GetUserSettingsAsync(userId);

            var viewModel = new LenderIndexViewModel
            {
                Lenders = lenders.Where(l => !l.IsArchived).ToList(),
                ArchivedLenders = lenders.Where(l => l.IsArchived).ToList(),
                DefaultLenderId = settings.DefaultLenderId
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading lenders page");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error loading lenders.";
            return View(new LenderIndexViewModel());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new LenderFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LenderFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dto = new CreateLenderDto
            {
                Name = model.Name,
                AnnualRate = model.AnnualRate / 100,
                OriginationFee = model.OriginationFee / 100,
                LoanServiceFee = model.LoanServiceFee / 100,
                Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim()
            };

            await _lenderService.CreateLenderAsync(userId, dto);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Lender added.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lender");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error creating lender.";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var lender = await _lenderService.GetUserLenderAsync(id, userId);
            if (lender == null)
            {
                TempData["NotificationType"] = "error";
                TempData["Notification"] = "Lender not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new LenderFormViewModel
            {
                Id = lender.Id,
                Name = lender.Name,
                AnnualRate = Math.Round(lender.AnnualRate * 100, 2),
                OriginationFee = Math.Round(lender.OriginationFee * 100, 2),
                LoanServiceFee = Math.Round(lender.LoanServiceFee * 100, 2),
                Note = lender.Note
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading lender {LenderId} for edit", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error loading lender.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LenderFormViewModel model)
    {
        if (!ModelState.IsValid || model.Id == null)
        {
            return View(model);
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dto = new UpdateLenderDto
            {
                Id = model.Id.Value,
                Name = model.Name,
                AnnualRate = model.AnnualRate / 100,
                OriginationFee = model.OriginationFee / 100,
                LoanServiceFee = model.LoanServiceFee / 100,
                Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim()
            };

            var updated = await _lenderService.UpdateLenderAsync(userId, dto);
            if (updated == null)
            {
                TempData["NotificationType"] = "error";
                TempData["Notification"] = "Lender not found.";
                return RedirectToAction(nameof(Index));
            }

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Lender updated.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lender {LenderId}", model.Id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error updating lender.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _lenderService.DeleteLenderAsync(userId, id);

            // Clear default lender if this was the default
            var settings = await _dealSettingsService.GetUserSettingsAsync(userId);
            if (settings.DefaultLenderId == id)
            {
                await _dealSettingsService.SetDefaultLenderAsync(userId, null);
            }

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Lender archived.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lender {LenderId}", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error deleting lender.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(int? lenderId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (lenderId.HasValue)
            {
                var lender = await _lenderService.GetUserLenderAsync(lenderId.Value, userId);
                if (lender == null)
                {
                    TempData["NotificationType"] = "error";
                    TempData["Notification"] = "Lender not found.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _dealSettingsService.SetDefaultLenderAsync(userId, lenderId);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = lenderId.HasValue ? "Default lender updated." : "Default lender cleared.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default lender {LenderId}", lenderId);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error updating default lender.";
            return RedirectToAction(nameof(Index));
        }
    }
}
