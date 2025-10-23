using System.Diagnostics;
using System.Security.Claims;
using DealEvaluator.Application.DTOs.Property;
using DealEvaluator.Application.DTOs.Zillow;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Application.Services;
using DealEvaluator.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealEvaluator.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ZillowApiService _zillowApiService;
    private readonly IPropertyService _propertyService;

    public HomeController(
        ILogger<HomeController> logger,
        ZillowApiService zillowApiService,
        IPropertyService propertyService)
    {
        _logger = logger;
        _zillowApiService = zillowApiService;
        _propertyService = propertyService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EvaluateProperty(CreatePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "Please fill in all required fields correctly.";
            return View("Index");
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Create or update property
            var property = await _propertyService.CreatePropertyAsync(dto, userId);

            // Run evaluation logic
            var evaluationResult = await _propertyService.EvaluatePropertyDealAsync(property.Id);

            // Show success message
            TempData["NotificationType"] = "success";
            TempData["Notification"] = $"Property evaluated successfully! ID: {property.Id}";

            // TODO: Create a Results view to display evaluation
            // TODO: Create property/deal profiles with results tab for all previous evaluations that you can open full page as "Results"
            // return RedirectToAction("Results", new { id = property.Id });

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating property");
            TempData["NotificationType"] = "error";
            TempData["Notification"] = "An unexpected error occurred while evaluating the property.";
            return View("Index");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}