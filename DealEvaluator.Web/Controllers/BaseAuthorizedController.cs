using DealEvaluator.Application.DTOs.Property;
using DealEvaluator.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DealEvaluator.Web.Controllers;

/// <summary>
/// Base controller with helper methods for authorized property access.
/// </summary>
public abstract class BaseAuthorizedController : Controller
{
    protected readonly IPropertyService PropertyService;
    protected readonly IAuthorizationService AuthorizationService;

    protected BaseAuthorizedController(
        IPropertyService propertyService,
        IAuthorizationService authorizationService)
    {
        PropertyService = propertyService;
        AuthorizationService = authorizationService;
    }

    /// <summary>
    /// Gets a property and checks if the current user is authorized to access it.
    /// Returns (null, property) if authorized, or (error, null) if not found/unauthorized.
    /// </summary>
    /// <param name="id">Property ID</param>
    /// <returns>Tuple of (error result, property). If error is null, property is guaranteed to exist and be authorized.</returns>
    protected async Task<(IActionResult? Error, PropertyDto? Property)> GetAuthorizedPropertyAsync(int id)
    {
        var property = await PropertyService.GetPropertyByIdAsync(id);
        if (property == null)
            return (NotFound(new { error = "Property not found" }), null);

        var authResult = await AuthorizationService.AuthorizeAsync(User, property, "PropertyOwner");
        if (!authResult.Succeeded)
            return (Forbid(), null);

        return (null, property);
    }
}