using System.Diagnostics;
using Deal_Evaluator.API;
using Deal_Evaluator.DTOs.Zillow;
using Microsoft.AspNetCore.Mvc;
using Deal_Evaluator.Models;
using Microsoft.AspNetCore.Authorization;

namespace Deal_Evaluator.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ZillowApiService _zillowApiService;

    public HomeController(ILogger<HomeController> logger, ZillowApiService zillowApiService)
    {
        _logger = logger;
        _zillowApiService = zillowApiService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Searches for properties using Zillow API.
    /// </summary>
    /// <param name="request">The search criteria.</param>
    /// <returns>List of properties matching the criteria.</returns>
    [HttpPost("search")]
    public async Task<ActionResult<ZillowSearchResponse>> SearchProperties([FromBody] ZillowSearchRequest request)
    {
        if (request == null) return BadRequest("Invalid request");
        
        var response = await _zillowApiService.SearchPropertiesAsync(request);
        
        return Ok(response);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}