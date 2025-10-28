using System.Security.Claims;
using DealEvaluator.Application.DTOs.Property;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Web.Models;
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
            await _propertyService.EvaluatePropertyDealAsync(property.Id);

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

            if (property.UserId != userId)
                return Forbid();

            // Get all evaluations for this property
            var evaluations = await _propertyService.GetPropertyEvaluationsAsync(id);
            var latestEvaluation = evaluations.FirstOrDefault();

            // Get all comparables for this property
            var comparables = await _propertyService.GetComparablesAsync(id);

            // Create ViewModel with property, evaluations, and comparables
            var viewModel = new PropertyDetailsViewModel
            {
                Property = property,
                LatestEvaluation = latestEvaluation,
                EvaluationHistory = evaluations.Skip(1).ToList(), // All except the latest
                Comparables = comparables
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading property details for ID {PropertyId}", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Property not found.";
            return RedirectToAction("Index");
        }
    }

    // GET: Property/Edit/5 - Show form to edit existing property
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var property = await _propertyService.GetPropertyByIdAsync(id);

            if (property.UserId != userId)
                return Forbid();

            // Map PropertyDto to UpdatePropertyDto for editing
            var updateDto = new UpdatePropertyDto
            {
                Id = property.Id,
                PropertyType = property.PropertyType,
                PropertyConditions = property.PropertyConditions,
                Address = property.Address,
                City = property.City,
                State = property.State,
                ZipCode = property.ZipCode,
                Price = property.Price,
                Sqft = property.Sqft,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                LotSizeSqft = property.LotSizeSqft,
                YearBuilt = property.YearBuilt
            };

            return View(updateDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading property for edit, ID {PropertyId}", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Property not found.";
            return RedirectToAction("Index");
        }
    }

    // POST: Property/Edit/5 - Update existing property and re-evaluate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePropertyDto dto)
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

            // Verify ownership
            var existingProperty = await _propertyService.GetPropertyByIdAsync(dto.Id);
            if (existingProperty.UserId != userId)
                return Forbid();

            // Update property
            var updatedProperty = await _propertyService.UpdatePropertyAsync(dto);

            // Re-run evaluation with updated data
            await _propertyService.EvaluatePropertyDealAsync(updatedProperty.Id);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Property updated and re-evaluated successfully!";

            return RedirectToAction("Details", new { id = updatedProperty.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property ID {PropertyId}", dto.Id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "An error occurred while updating the property.";
            return View(dto);
        }
    }

    // POST: Property/Evaluate/5 - Re-evaluate a property
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Evaluate(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var property = await _propertyService.GetPropertyByIdAsync(id);

            if (property.UserId != userId)
                return Forbid();

            // Re-run evaluation logic
            await _propertyService.EvaluatePropertyDealAsync(id);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Property re-evaluated successfully!";

            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating property ID {PropertyId}", id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error evaluating property.";
            return RedirectToAction("Details", new { id });
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

            var property = await _propertyService.GetPropertyByIdAsync(id);

            if (property.UserId != userId)
                return Forbid();

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

    // POST: Property/DeleteComparable - Delete a comparable
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComparable(int comparableId, int propertyId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify property ownership
            var property = await _propertyService.GetPropertyByIdAsync(propertyId);
            if (property.UserId != userId)
                return Forbid();

            await _propertyService.DeleteComparableAsync(comparableId);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Comparable deleted successfully.";

            return RedirectToAction("Details", new { id = propertyId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comparable ID {ComparableId}", comparableId);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error deleting comparable.";
            return RedirectToAction("Details", new { id = propertyId });
        }
    }
}