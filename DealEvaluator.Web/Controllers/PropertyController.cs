using System.Security.Claims;
using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Application.DTOs.Evaluation;
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
    private readonly IMarketDataService _marketDataService;

    public PropertyController(
        ILogger<PropertyController> logger,
        IPropertyService propertyService,
        IMarketDataService marketDataService)
    {
        _logger = logger;
        _propertyService = propertyService;
        _marketDataService = marketDataService;
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

            // Create property (includes automatic placeholder evaluation if repair cost provided)
            var property = await _propertyService.CreatePropertyAsync(dto, userId);

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

            TempData["NotificationType"] = "success";
            TempData["Notification"] = "Property updated successfully!";

            // Redirect with flag to prompt for new evaluation
            return RedirectToAction("Details", new { id = updatedProperty.Id, promptEvaluation = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating property ID {PropertyId}", dto.Id);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "An error occurred while updating the property.";
            return View(dto);
        }
    }

    // POST: Property/CreateEvaluation - Create a new evaluation with 70% rule calculations
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvaluation(CreateEvaluationDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var property = await _propertyService.GetPropertyByIdAsync(dto.PropertyId);

            if (property == null)
            {
                TempData["NotificationType"] = "error";
                TempData["Notification"] = "Property not found.";
                return RedirectToAction("Index");
            }

            if (property.UserId != userId)
                return Forbid();

            var evaluation = await _propertyService.CreateEvaluationAsync(dto);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = $"Evaluation created! Max Offer: ${evaluation.MaxOffer:N0}, Potential Profit: ${evaluation.Profit:N0}";

            return RedirectToAction("Details", new { id = dto.PropertyId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating evaluation for property {PropertyId}", dto.PropertyId);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = ex.Message;
            return RedirectToAction("Details", new { id = dto.PropertyId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating evaluation for property ID {PropertyId}", dto.PropertyId);
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Error creating evaluation. Please try again.";
            return RedirectToAction("Details", new { id = dto.PropertyId });
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

    // POST: Property/AddComparable - Add a comparable from market data
    [HttpPost]
    public async Task<IActionResult> AddComparable([FromBody] CreateComparableDto dto)
    {
        try
        {
            if (dto == null)
            {
                _logger.LogError("AddComparable: dto is null");
                return Json(new { success = false, error = "Invalid data received" });
            }

            _logger.LogInformation("AddComparable called with PropertyId: {PropertyId}, Address: {Address}",
                dto.PropertyId, dto.Address);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify property ownership
            var property = await _propertyService.GetPropertyByIdAsync(dto.PropertyId);
            if (property == null)
            {
                _logger.LogError("Property not found: {PropertyId}", dto.PropertyId);
                return Json(new { success = false, error = "Property not found" });
            }

            if (property.UserId != userId)
            {
                _logger.LogError("Unauthorized access attempt for property {PropertyId} by user {UserId}",
                    dto.PropertyId, userId);
                return Json(new { success = false, error = "Unauthorized" });
            }

            var comparable = await _propertyService.CreateComparableFromMarketDataAsync(dto);

            _logger.LogInformation("Successfully added comparable {ComparableId} to property {PropertyId}",
                comparable.Id, dto.PropertyId);

            return Json(new { success = true, comparable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comparable for property ID {PropertyId}. DTO: {@Dto}",
                dto?.PropertyId, dto);
            return Json(new { success = false, error = $"Failed to add comparable: {ex.Message}" });
        }
    }

    // GET: /Property/GetMarketData?zipCode=12345 - API endpoint for fetching market data
    [HttpGet]
    public async Task<IActionResult> GetMarketData(string zipCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return BadRequest(new { error = "Zip code is required" });
            }

            var marketData = await _marketDataService.GetMarketDataForZipCodeAsync(zipCode);

            return Json(new
            {
                success = true,
                properties = marketData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching market data for zip code: {ZipCode}", zipCode);
            return Json(new
            {
                success = false,
                error = "Failed to fetch market data"
            });
        }
    }

}