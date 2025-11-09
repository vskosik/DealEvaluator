using Microsoft.AspNetCore.Authorization;

namespace DealEvaluator.Application.Authorization;

/// <summary>
/// Authorization requirement for property ownership.
/// Requires that the user is either the owner of the property or an admin.
/// </summary>
public class PropertyOwnerRequirement : IAuthorizationRequirement
{
}