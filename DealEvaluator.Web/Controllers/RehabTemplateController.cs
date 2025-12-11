using System.Security.Claims;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealEvaluator.Web.Controllers;

[Authorize]
public class RehabTemplateController : Controller
{
    private readonly ILogger<RehabTemplateController> _logger;
    private readonly IRehabCostTemplateService _templateService;

    public RehabTemplateController(
        ILogger<RehabTemplateController> logger,
        IRehabCostTemplateService templateService)
    {
        _logger = logger;
        _templateService = templateService;
    }

    // POST: RehabTemplate/Upsert - Create or update a cost template
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(RehabLineItemType lineItemType, RehabCondition condition, decimal defaultCost)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _templateService.UpsertTemplateAsync(userId, lineItemType, condition, defaultCost);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Cost template updated successfully!";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cost template");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error updating cost template.";
            return RedirectToAction("Index");
        }
    }

    // POST: RehabTemplate/Delete - Delete a cost template
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _templateService.DeleteTemplateAsync(id);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Cost template deleted successfully.";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cost template ID {TemplateId}", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error deleting cost template.";
            return RedirectToAction("Index");
        }
    }

    // POST: RehabTemplate/SeedDefaults - Seed default templates for user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedDefaults()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _templateService.SeedDefaultTemplatesAsync(userId);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Default cost templates created successfully!";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default cost templates");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error creating default cost templates.";
            return RedirectToAction("Index");
        }
    }

    // GET: RehabTemplate/GetTemplate - API endpoint for fetching a specific template
    [HttpGet]
    public async Task<IActionResult> GetTemplate(RehabLineItemType lineItemType, RehabCondition condition)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var template = await _templateService.GetTemplateAsync(userId, lineItemType, condition);

            if (template == null)
                return Json(new { success = false, error = "Template not found" });

            return Json(new { success = true, template });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cost template");
            return Json(new { success = false, error = "Error fetching template" });
        }
    }
}