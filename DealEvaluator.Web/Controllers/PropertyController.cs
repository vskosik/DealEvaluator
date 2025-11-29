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
public class PropertyController : BaseAuthorizedController
{
    private readonly ILogger<PropertyController> _logger;
    private readonly IMarketDataService _marketDataService;

    public PropertyController(
        ILogger<PropertyController> logger,
        IPropertyService propertyService,
        IAuthorizationService authorizationService,
        IMarketDataService marketDataService) : base(propertyService, authorizationService)
    {
        _logger = logger;
        _marketDataService = marketDataService;
    }

    // GET: Property/Index - List all user's properties
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var properties = await PropertyService.GetUserPropertiesAsync(userId);

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
            var property = await PropertyService.CreatePropertyAsync(dto, userId);

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
            var (error, property) = await GetAuthorizedPropertyAsync(id);
            if (error != null) return error;

            // Get all evaluations for this property
            var evaluations = await PropertyService.GetPropertyEvaluationsAsync(id);
            var latestEvaluation = evaluations.FirstOrDefault();

            // Get all comparables for this property
            var comparables = await PropertyService.GetComparablesAsync(id);

            // Create ViewModel with property, evaluations, and comparables
            var viewModel = new PropertyDetailsViewModel
            {
                Property = property!,
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
            TempData["Notification"] = "Error loading property details.";
            return RedirectToAction("Index");
        }
    }

    // GET: Property/Edit/5 - Show form to edit existing property
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var (error, property) = await GetAuthorizedPropertyAsync(id);
            if (error != null) return error;

            // Map Property to UpdatePropertyDto for editing
            var updateDto = new UpdatePropertyDto
            {
                Id = property!.Id,
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
            TempData["Notification"] = "Error loading property for edit.";
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
            var (error, _) = await GetAuthorizedPropertyAsync(dto.Id);
            if (error != null) return error;

            // Update property
            var updatedProperty = await PropertyService.UpdatePropertyAsync(dto);

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

    // POST: Property/Delete/5 - Delete a property
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var (error, _) = await GetAuthorizedPropertyAsync(id);
            if (error != null) return error;

            await PropertyService.DeletePropertyAsync(id);

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
            var (error, _) = await GetAuthorizedPropertyAsync(propertyId);
            if (error != null) return error;

            await PropertyService.DeleteComparableAsync(comparableId);

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

            var (error, _) = await GetAuthorizedPropertyAsync(dto.PropertyId);
            if (error != null)
                return Json(new { success = false, error = "Unauthorized or property not found" });

            var comparable = await PropertyService.CreateComparableFromMarketDataAsync(dto);

            return Json(new { success = true, comparable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comparable for property ID {PropertyId}. DTO: {@Dto}",
                dto?.PropertyId, dto);
            return Json(new { success = false, error = $"Failed to add comparable: {ex.Message}" });
        }
    }
    
    // POST: Property/CreateEvaluation - Create a new evaluation with 70% rule calculations
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvaluation(CreateEvaluationDto dto)
    {
        try
        {
            var (error, _) = await GetAuthorizedPropertyAsync(dto.PropertyId);
            if (error != null) return error;

            // Create evaluation
            var evaluation = await PropertyService.CreateEvaluationAsync(dto);

            TempData["NotificationType"] = "success";
            TempData["Notification"] = $"Evaluation created! Max Offer: ${evaluation.MaxOffer:N0}, Potential Profit: ${evaluation.Profit:N0}";

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

    // GET: /Property/GetMarketData?zipCode=12345 - API endpoint for fetching market data
    [HttpGet]
    public async Task<IActionResult> GetMarketData(string zipCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return BadRequest("Zip code is required");
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

    // GET: /Property/GetZipCodeBoundaries?lat=40.7128&lng=-74.0060&radiusMeters=80467 - API endpoint for fetching zip code boundaries
    [HttpGet]
    public async Task<IActionResult> GetZipCodeBoundaries(double lat, double lng, int radiusMeters)
    {
        try
        {
            _logger.LogInformation("Fetching zip code boundaries for lat={Lat}, lng={Lng}, radius={Radius}m", lat, lng, radiusMeters);

            // Load local GeoJSON files for NJ and NY
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data");
            var njPath = Path.Combine(webRoot, "nj_new_jersey_zip_codes_geo.min.json");
            var nyPath = Path.Combine(webRoot, "ny_new_york_zip_codes_geo.min.json");

            var allFeatures = new List<object>();

            // Load NJ boundaries
            if (System.IO.File.Exists(njPath))
            {
                var njJson = await System.IO.File.ReadAllTextAsync(njPath);
                var njData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(njJson);

                if (njData.TryGetProperty("features", out var njFeatures))
                {
                    foreach (var feature in njFeatures.EnumerateArray())
                    {
                        if (IsFeatureWithinRadius(feature, lat, lng, radiusMeters))
                        {
                            allFeatures.Add(System.Text.Json.JsonSerializer.Deserialize<object>(feature.GetRawText()));
                        }
                    }
                }
            }

            // Load NY boundaries
            if (System.IO.File.Exists(nyPath))
            {
                var nyJson = await System.IO.File.ReadAllTextAsync(nyPath);
                var nyData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(nyJson);

                if (nyData.TryGetProperty("features", out var nyFeatures))
                {
                    foreach (var feature in nyFeatures.EnumerateArray())
                    {
                        if (IsFeatureWithinRadius(feature, lat, lng, radiusMeters))
                        {
                            allFeatures.Add(System.Text.Json.JsonSerializer.Deserialize<object>(feature.GetRawText()));
                        }
                    }
                }
            }

            _logger.LogInformation("Found {Count} zip code boundaries within radius", allFeatures.Count);

            return Json(new
            {
                success = true,
                data = new
                {
                    type = "FeatureCollection",
                    features = allFeatures
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching zip code boundaries for lat={Lat}, lng={Lng}", lat, lng);
            return Json(new
            {
                success = false,
                error = "Failed to fetch zip code boundaries"
            });
        }
    }

    private bool IsFeatureWithinRadius(System.Text.Json.JsonElement feature, double centerLat, double centerLng, double radiusMeters)
    {
        try
        {
            // Get the geometry from the feature
            if (!feature.TryGetProperty("geometry", out var geometry)) return false;
            if (!geometry.TryGetProperty("coordinates", out var coordinates)) return false;

            // For polygons, get the first ring and calculate centroid
            var points = new List<(double lat, double lng)>();

            if (geometry.TryGetProperty("type", out var geoType) && geoType.GetString() == "Polygon")
            {
                // First array is the outer ring
                foreach (var ring in coordinates.EnumerateArray())
                {
                    foreach (var point in ring.EnumerateArray())
                    {
                        var coords = point.EnumerateArray().ToList();
                        if (coords.Count >= 2)
                        {
                            points.Add((coords[1].GetDouble(), coords[0].GetDouble())); // lat, lng
                        }
                    }
                    break; // Only process first ring
                }
            }

            if (points.Count == 0) return false;

            // Calculate centroid
            var centroidLat = points.Average(p => p.lat);
            var centroidLng = points.Average(p => p.lng);

            // Calculate distance from center to centroid
            var distance = CalculateDistance(centerLat, centerLng, centroidLat, centroidLng);

            return distance <= radiusMeters;
        }
        catch
        {
            return false;
        }
    }

    private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
    {
        // Haversine formula to calculate distance in meters
        const double R = 6371000; // Earth's radius in meters

        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    // GET: /Property/GeocodeZipCode?zipCode=12345 - API endpoint for geocoding a zip code
    [HttpGet]
    public async Task<IActionResult> GeocodeZipCode(string zipCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(zipCode))
            {
                return BadRequest("Zip code is required");
            }

            _logger.LogInformation("Geocoding zip code: {ZipCode}", zipCode);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DealEvaluator/1.0");

            var response = await httpClient.GetAsync(
                $"https://nominatim.openstreetmap.org/search?postalcode={zipCode}&country=US&format=json&limit=1"
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Nominatim API returned status code: {StatusCode}", response.StatusCode);
                return Json(new
                {
                    success = false,
                    error = $"Nominatim API error: {response.StatusCode}"
                });
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            return Json(new
            {
                success = true,
                data = System.Text.Json.JsonSerializer.Deserialize<object>(responseContent)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding zip code: {ZipCode}", zipCode);
            return Json(new
            {
                success = false,
                error = "Failed to geocode zip code"
            });
        }
    }

    // GET: /Property/GetAutoRehabEstimate?propertyId=5 - API endpoint for getting auto-generated rehab estimate
    [HttpGet]
    public async Task<IActionResult> GetAutoRehabEstimate(int propertyId)
    {
        try
        {
            var (error, _) = await GetAuthorizedPropertyAsync(propertyId);
            if (error != null)
            {
                return Json(new { success = false, error = "Unauthorized or property not found" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var lineItems = await PropertyService.GetAutoGeneratedRehabEstimateAsync(propertyId, userId);

            return Json(new
            {
                success = true,
                lineItems
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating auto rehab estimate for property ID {PropertyId}", propertyId);
            return Json(new
            {
                success = false,
                error = "Failed to generate rehab estimate"
            });
        }
    }

}