using System.Security.Claims;
using DealEvaluator.Application.DTOs.Property;
using Microsoft.AspNetCore.Authorization;

namespace DealEvaluator.Application.Authorization;

/// <summary>
/// Handler that checks if a user can access a property.
/// Allows access if the user is the owner or has the Admin role.
/// </summary>
public class PropertyOwnershipHandler : AuthorizationHandler<PropertyOwnerRequirement, PropertyDto>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PropertyOwnerRequirement requirement,
        PropertyDto resource)
    {
        // Get the current user's ID
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            // User is not authenticated - fail
            return Task.CompletedTask;
        }

        // Check if user is an admin - admins can access all properties
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user is the owner of the property
        if (resource.UserId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}