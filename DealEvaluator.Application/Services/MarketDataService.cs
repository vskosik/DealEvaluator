using System.Text.Json;
using DealEvaluator.Application.DTOs.Zillow;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DealEvaluator.Application.Services;

public class MarketDataService : IMarketDataService
{
    private readonly IMarketDataRepository _marketDataRepository;
    private readonly ZillowApiService _zillowApiService;
    private readonly ILogger<MarketDataService> _logger;

    public MarketDataService(
        IMarketDataRepository marketDataRepository,
        ZillowApiService zillowApiService,
        ILogger<MarketDataService> logger)
    {
        _marketDataRepository = marketDataRepository;
        _zillowApiService = zillowApiService;
        _logger = logger;
    }

    public async Task<List<ZillowProperty>> GetMarketDataForZipCodeAsync(string zipCode)
    {
        _logger.LogInformation("Fetching market data for zip code: {ZipCode}", zipCode);
        
        var cachedData = await _marketDataRepository.GetByZipCodeAsync(zipCode);

        if (cachedData != null && !IsExpired(cachedData))
        {
            _logger.LogInformation("Using cached market data for zip code: {ZipCode}", zipCode);
            return ParseRawJson(cachedData.RawJson);
        }

        _logger.LogInformation("Cache miss or expired for zip code: {ZipCode}. Fetching from Zillow API.", zipCode);
        
        return await RefreshMarketDataAsync(zipCode);
    }

    public async Task<List<ZillowProperty>> RefreshMarketDataAsync(string zipCode)
    {
        _logger.LogInformation("Refreshing market data from Zillow API for zip code: {ZipCode}", zipCode);

        // Default search results
        var searchRequest = new ZillowSearchRequest
        {
            Location = zipCode,
            StatusType = ZillowStatusType.RecentlySold,
            HomeType = ZillowHomeType.Houses,
            Sort = ZillowSort.Newest,
            SoldInLast = "12m"
        };

        var response = await _zillowApiService.SearchPropertiesAsync(searchRequest);

        if (response?.Properties == null || response.Properties.Count == 0)
        {
            _logger.LogWarning("No properties found for zip code: {ZipCode}", zipCode);
            return new List<ZillowProperty>();
        }

        _logger.LogInformation("Found {Count} properties for zip code: {ZipCode}", response.Properties.Count, zipCode);
        
        var marketData = new MarketData
        {
            ZipCode = zipCode,
            Source = "Zillow",
            RawJson = JsonSerializer.Serialize(response.Properties),
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // Cache for 30 days
        };

        await _marketDataRepository.UpsertAsync(marketData);

        return response.Properties;
    }

    public async Task<bool> HasFreshDataAsync(string zipCode)
    {
        return await _marketDataRepository.IsFreshDataAvailableAsync(zipCode);
    }

    private bool IsExpired(MarketData marketData)
    {
        // If ExpiresAt is not set, consider it never expires
        if (marketData.ExpiresAt == null)
            return false;

        return marketData.ExpiresAt.Value < DateTime.UtcNow;
    }

    private List<ZillowProperty> ParseRawJson(string rawJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<ZillowProperty>>(rawJson)
                   ?? new List<ZillowProperty>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse cached market data JSON");
            return new List<ZillowProperty>();
        }
    }
}