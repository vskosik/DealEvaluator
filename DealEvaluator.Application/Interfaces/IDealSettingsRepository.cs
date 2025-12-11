using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface IDealSettingsRepository : IRepository<DealSettings>
{
    /// <summary>
    /// Gets the deal settings for a specific user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>The user's settings, or null if not found</returns>
    Task<DealSettings?> GetByUserIdAsync(string userId);
}