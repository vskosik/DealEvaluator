using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface IDealSettingsService
{
    /// <summary>
    /// Gets the deal settings for a user. Creates default settings if user doesn't have any.
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>The user's settings (existing or newly created defaults)</returns>
    Task<DealSettings> GetUserSettingsAsync(string userId);

    /// <summary>
    /// Updates the deal settings for a user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="settings">The updated settings</param>
    /// <returns>The updated settings</returns>
    Task<DealSettings> UpdateSettingsAsync(string userId, DealSettings settings);

    /// <summary>
    /// Resets a user's settings to default values
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>The reset settings with default values</returns>
    Task<DealSettings> ResetToDefaultsAsync(string userId);

    /// <summary>
    /// Sets or clears the default lender for a user
    /// </summary>
    Task<DealSettings> SetDefaultLenderAsync(string userId, int? lenderId);
}
