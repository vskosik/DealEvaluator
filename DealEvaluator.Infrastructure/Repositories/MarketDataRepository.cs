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
        _marketDatas =  _context.MarketData;
    }

    public async Task<MarketData?> GetByZipCodeAndKeywordsAsync(string zipCode, string homeType, string keywords)
    {
        return await _marketDatas
            .FirstOrDefaultAsync(m => m.ZipCode == zipCode && m.HomeType == homeType && m.Keywords == keywords);
    }

    public async Task UpsertAsync(MarketData marketData)
    {
        var existing = await GetByZipCodeAndKeywordsAsync(marketData.ZipCode, marketData.HomeType, marketData.Keywords);

        if (existing != null)
        {
            // Update existing record
            existing.RawJson = marketData.RawJson;
            existing.Source = marketData.Source;
            existing.FetchedAt = marketData.FetchedAt;
            existing.ExpiresAt = marketData.ExpiresAt;

            _marketDatas.Update(existing);
        }
        else
        {
            // Insert new record
            await _marketDatas.AddAsync(marketData);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsFreshDataAvailableAsync(string zipCode, string homeType, string keywords)
    {
        var data = await GetByZipCodeAndKeywordsAsync(zipCode, homeType, keywords);

        if (data == null)
            return false;

        return data.ExpiresAt == null || data.ExpiresAt > DateTime.UtcNow;
    }
}