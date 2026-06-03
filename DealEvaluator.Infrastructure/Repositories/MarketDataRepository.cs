using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class MarketDataRepository : DbRepository<MarketData>, IMarketDataRepository
{
    private readonly DealEvaluatorContext _context;
    private readonly DbSet<MarketData> _marketDatas;

    public MarketDataRepository(DealEvaluatorContext context) : base(context)
    {
        _context = context;
        _marketDatas = _context.MarketData;
    }

    public async Task<MarketData?> GetByZipCodeAndKeywordsAsync(string zipCode, string homeType, string keywords)
    {
        return await _marketDatas
            .FirstOrDefaultAsync(m => m.ZipCode == zipCode && m.HomeType == homeType && m.Keywords == keywords);
    }

    public async Task UpsertAsync(MarketData marketData, List<CachedProperty> properties)
    {
        var existing = await GetByZipCodeAndKeywordsAsync(marketData.ZipCode, marketData.HomeType, marketData.Keywords);

        if (existing != null)
        {
            existing.Source = marketData.Source;
            existing.FetchedAt = marketData.FetchedAt;
            existing.ExpiresAt = marketData.ExpiresAt;
            _marketDatas.Update(existing);

            var oldProperties = await _context.CachedProperties
                .Where(p => p.MarketDataId == existing.Id)
                .ToListAsync();
            _context.CachedProperties.RemoveRange(oldProperties);

            foreach (var p in properties)
                p.MarketDataId = existing.Id;

            await _context.CachedProperties.AddRangeAsync(properties);
        }
        else
        {
            await _marketDatas.AddAsync(marketData);
            await _context.SaveChangesAsync();

            foreach (var p in properties)
                p.MarketDataId = marketData.Id;

            await _context.CachedProperties.AddRangeAsync(properties);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsFreshDataAvailableAsync(string zipCode, string homeType, string keywords)
    {
        var data = await GetByZipCodeAndKeywordsAsync(zipCode, homeType, keywords);
        if (data == null) return false;
        return data.ExpiresAt == null || data.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<List<CachedProperty>> GetPropertiesAsync(string zipCode, string homeType, string keywords)
    {
        return await _context.CachedProperties
            .Where(p => _context.MarketData
                .Any(m => m.Id == p.MarketDataId
                          && m.ZipCode == zipCode
                          && m.HomeType == homeType
                          && m.Keywords == keywords))
            .ToListAsync();
    }
}
