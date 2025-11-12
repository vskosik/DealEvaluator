using DealEvaluator.Domain.Entities;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Interfaces;

public interface IRehabCostTemplateRepository : IRepository<RehabCostTemplate>
{
    /// <summary>
    /// Gets all cost templates for a specific user
    /// </summary>
    Task<List<RehabCostTemplate>> GetTemplatesByUserIdAsync(string userId);

    /// <summary>
    /// Gets a specific template by user, line item type, and condition
    /// </summary>
    Task<RehabCostTemplate?> GetTemplateByUserAndTypeConditionAsync(string userId, RehabLineItemType lineItemType, RehabCondition condition);

    /// <summary>
    /// Gets all templates for a specific line item type across all conditions for a user
    /// </summary>
    Task<List<RehabCostTemplate>> GetTemplatesByUserAndTypeAsync(string userId, RehabLineItemType lineItemType);
}