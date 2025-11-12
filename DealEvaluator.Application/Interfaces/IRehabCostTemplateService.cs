using DealEvaluator.Application.DTOs.Rehab;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Interfaces;

/// <summary>
/// Service for managing rehab cost templates
/// </summary>
public interface IRehabCostTemplateService
{
    /// <summary>
    /// Gets all cost templates for a specific user
    /// </summary>
    Task<List<RehabCostTemplateDto>> GetUserTemplatesAsync(string userId);

    /// <summary>
    /// Gets a specific template by line item type and condition for a user
    /// </summary>
    Task<RehabCostTemplateDto?> GetTemplateAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition);

    /// <summary>
    /// Creates or updates a cost template for a user
    /// </summary>
    Task<RehabCostTemplateDto> UpsertTemplateAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition, decimal defaultCost);

    /// <summary>
    /// Deletes a specific cost template
    /// </summary>
    Task DeleteTemplateAsync(int templateId);

    /// <summary>
    /// Seeds default cost templates for a new user
    /// </summary>
    Task SeedDefaultTemplatesAsync(string userId);
}