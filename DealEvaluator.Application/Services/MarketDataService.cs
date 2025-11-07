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

    public async Task<List<ZillowProperty>> GetMarketDataForZipCodeAsync(string zipCode, string keywords = "")
    {
        _logger.LogInformation("Fetching market data for zip code: {ZipCode}, keywords: {Keywords}", zipCode, keywords);

        var cachedData = await _marketDataRepository.GetByZipCodeAndKeywordsAsync(zipCode, keywords);

        if (cachedData != null && !IsExpired(cachedData))
        {
            _logger.LogInformation("Using cached market data for zip code: {ZipCode}, keywords: {Keywords}", zipCode, keywords);
            return ParseRawJson(cachedData.RawJson);
        }

        _logger.LogInformation("Cache miss or expired for zip code: {ZipCode}, keywords: {Keywords}. Fetching from Zillow API.", zipCode, keywords);

        return await RefreshMarketDataAsync(zipCode, keywords);
    }

    public async Task<List<ZillowProperty>> RefreshMarketDataAsync(string zipCode, string keywords = "")
    {
        _logger.LogInformation("Refreshing market data from Zillow API for zip code: {ZipCode}, keywords: {Keywords}", zipCode, keywords);

        // Default search results
        var searchRequest = new ZillowSearchRequest
        {
            Location = zipCode,
            StatusType = ZillowStatusType.RecentlySold,
            HomeType = ZillowHomeType.Houses,
            Sort = ZillowSort.Newest,
            SoldInLast = "12m",
            Keywords = string.IsNullOrWhiteSpace(keywords) ? null : keywords
        };

        var response = await _zillowApiService.SearchPropertiesAsync(searchRequest);

        if (response?.Properties == null || response.Properties.Count == 0)
        {
            _logger.LogWarning("No properties found for zip code: {ZipCode}, keywords: {Keywords}", zipCode, keywords);
            return new List<ZillowProperty>();
        }

        _logger.LogInformation("Found {Count} properties for zip code: {ZipCode}, keywords: {Keywords}", response.Properties.Count, zipCode, keywords);

        var marketData = new MarketData
        {
            ZipCode = zipCode,
            Keywords = keywords,
            Source = "Zillow",
            RawJson = JsonSerializer.Serialize(response.Properties),
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // Cache for 30 days
        };

        await _marketDataRepository.UpsertAsync(marketData);

        return response.Properties;
    }

    public async Task<bool> HasFreshDataAsync(string zipCode, string keywords = "")
    {
        return await _marketDataRepository.IsFreshDataAvailableAsync(zipCode, keywords);
    }

    // TODO: Implement cache expiration logic to automatically refresh stale data
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