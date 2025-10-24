using System.Security.Claims;
using DealEvaluator.Application.DTOs.Property;
using DealEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealEvaluator.Web.Controllers;

[Authorize]
public class PropertyController : Controller
{
    private readonly ILogger<PropertyController> _logger;
    private readonly IPropertyService _propertyService;

    public PropertyController(
        ILogger<PropertyController> logger,
        IPropertyService propertyService)
    {
        _logger = logger;
        _propertyService = propertyService;
    }

    // GET: Property/Index - List all user's properties
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var properties = await _propertyService.GetUserPropertiesAsync(userId);

            return View(properties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading properties");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error loading your properties.";
            return View(new List<PropertyDto>());
        }
    }

    // GET: Property/Create - Show form to create new property
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Property/Create - Create and evaluate new property
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Please fill in all required fields correctly.";
            return View(dto);
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Create property
            var property = await _propertyService.CreatePropertyAsync(dto, userId);

            // Run evaluation logic
            var evaluationResult = await _propertyService.EvaluatePropertyDealAsync(property.Id);

            // Show success message
            TempData["NotificationType"] = "success";
            TempData["Notification"] = $"Property created and evaluated successfully!";

            // Redirect to property details to show evaluation results
            return RedirectToAction("Details", new { id = property.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating property");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "An error occurred while creating the property.";
            return View(dto);
        }
    }

    // GET: Property/Details/5 - View property details and evaluation results
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var property = await _propertyService.GetPropertyByIdAsync(id);

            // TODO: Add authorization check to ensure user owns this property
            // For now, we'll allow any authenticated user to view any property
            // In production, you'd want: if (property.UserId != userId) return Forbid();

            return View(property);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading property details for ID {PropertyId}", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Property not found.";
            return RedirectToAction("Index");
        }
    }

    // POST: Property/Delete/5 - Delete a property
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // TODO: Add authorization check to ensure user owns this property

            await _propertyService.DeletePropertyAsync(id);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Property deleted successfully.";

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting property ID {PropertyId}", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error deleting property.";
            return RedirectToAction("Index");
        }
    }
}