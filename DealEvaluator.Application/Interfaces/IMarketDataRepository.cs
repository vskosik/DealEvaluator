using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface IMarketDataRepository : IRepository<MarketData>
{
    Task<MarketData?> GetByZipCodeAndKeywordsAsync(string zipCode, string homeType, string keywords);

    Task UpsertAsync(MarketData marketData, List<CachedProperty> properties);

    Task<bool> IsFreshDataAvailableAsync(string zipCode, string homeType, string keywords);

    Task<List<CachedProperty>> GetPropertiesAsync(string zipCode, string homeType, string keywords);
}