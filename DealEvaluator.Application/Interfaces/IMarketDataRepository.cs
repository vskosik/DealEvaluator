using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface IMarketDataRepository : IRepository<MarketData>
{
    /// <summary>
    /// Gets market data for a specific zip code, home type, and keywords combination
    /// </summary>
    Task<MarketData?> GetByZipCodeAndKeywordsAsync(string zipCode, string homeType, string keywords);

    /// <summary>
    /// Creates or updates market data for a zip code, home type, and keywords combination
    /// </summary>
    Task UpsertAsync(MarketData marketData);

    /// <summary>
    /// Checks if market data exists and is not expired for a specific zip code, home type, and keywords combination
    /// </summary>
    Task<bool> IsFreshDataAvailableAsync(string zipCode, string homeType, string keywords);
}