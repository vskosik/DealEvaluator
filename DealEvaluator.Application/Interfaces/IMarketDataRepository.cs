using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface IMarketDataRepository : IRepository<MarketData>
{
    /// <summary>
    /// Gets market data for a specific zip code
    /// </summary>
    Task<MarketData?> GetByZipCodeAsync(string zipCode);

    /// <summary>
    /// Creates or updates market data for a zip code
    /// </summary>
    Task UpsertAsync(MarketData marketData);

    /// <summary>
    /// Checks if market data exists and is not expired
    /// </summary>
    Task<bool> IsFreshDataAvailableAsync(string zipCode);
}