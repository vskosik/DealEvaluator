using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Services;

public class DealSettingsService : IDealSettingsService
{
    private readonly IDealSettingsRepository _repository;

    public DealSettingsService(IDealSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<DealSettings> GetUserSettingsAsync(string userId)
    {
        var settings = await _repository.GetByUserIdAsync(userId);

        if (settings != null)
            return settings;

        // Create default settings for new user
        var defaultSettings = CreateDefaultSettings(userId);
        await _repository.AddAsync(defaultSettings);
        await _repository.SaveChangesAsync();

        return defaultSettings;
    }

    public async Task<DealSettings> UpdateSettingsAsync(string userId, DealSettings settings)
    {
        var existingSettings = await _repository.GetByUserIdAsync(userId);

        if (existingSettings == null)
        {
            // User doesn't have settings yet, create new
            settings.UserId = userId;
            settings.CreatedAt = DateTime.UtcNow;
            settings.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(settings);
        }
        else
        {
            // Update existing settings
            existingSettings.SellingAgentCommission = settings.SellingAgentCommission;
            existingSettings.SellingClosingCosts = settings.SellingClosingCosts;
            existingSettings.BuyingClosingCosts = settings.BuyingClosingCosts;
            existingSettings.AnnualPropertyTaxRate = settings.AnnualPropertyTaxRate;
            existingSettings.MonthlyInsurance = settings.MonthlyInsurance;
            existingSettings.MonthlyUtilities = settings.MonthlyUtilities;
            existingSettings.DefaultHoldingMonths = settings.DefaultHoldingMonths;
            existingSettings.ProfitTargetType = settings.ProfitTargetType;
            existingSettings.ProfitTargetValue = settings.ProfitTargetValue;
            existingSettings.ContingencyPercentage = settings.ContingencyPercentage;
            existingSettings.UpdatedAt = DateTime.UtcNow;

            _repository.Update(existingSettings);
        }

        await _repository.SaveChangesAsync();

        return existingSettings ?? settings;
    }

    public async Task<DealSettings> ResetToDefaultsAsync(string userId)
    {
        var settings = await _repository.GetByUserIdAsync(userId);

        if (settings == null)
        {
            // No settings exist, create default
            return await GetUserSettingsAsync(userId);
        }

        // Reset to defaults
        var defaults = CreateDefaultSettings(userId);

        settings.SellingAgentCommission = defaults.SellingAgentCommission;
        settings.SellingClosingCosts = defaults.SellingClosingCosts;
        settings.BuyingClosingCosts = defaults.BuyingClosingCosts;
        settings.AnnualPropertyTaxRate = defaults.AnnualPropertyTaxRate;
        settings.MonthlyInsurance = defaults.MonthlyInsurance;
        settings.MonthlyUtilities = defaults.MonthlyUtilities;
        settings.DefaultHoldingMonths = defaults.DefaultHoldingMonths;
        settings.ProfitTargetType = defaults.ProfitTargetType;
        settings.ProfitTargetValue = defaults.ProfitTargetValue;
        settings.ContingencyPercentage = defaults.ContingencyPercentage;
        settings.UpdatedAt = DateTime.UtcNow;

        _repository.Update(settings);
        await _repository.SaveChangesAsync();

        return settings;
    }

    private static DealSettings CreateDefaultSettings(string userId)
    {
        return new DealSettings
        {
            UserId = userId,
            SellingAgentCommission = 0.06m,      // 6%
            SellingClosingCosts = 0.02m,         // 2%
            BuyingClosingCosts = 0.02m,          // 2%
            AnnualPropertyTaxRate = 0.012m,      // 1.2%
            MonthlyInsurance = 150m,             // $150/month
            MonthlyUtilities = 200m,             // $200/month
            DefaultHoldingMonths = 4,            // 4 months
            ProfitTargetType = ProfitTargetType.PercentageOfArv,
            ProfitTargetValue = 0.15m,           // 15% of ARV
            ContingencyPercentage = 0.10m,       // 10% of rehab costs
            CreatedAt = DateTime.UtcNow
        };
    }
}