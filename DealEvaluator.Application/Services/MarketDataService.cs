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

    public async Task<List<ZillowProperty>> GetMarketDataForZipCodeAsync(string zipCode, string homeType, string keywords = "")
    {
        _logger.LogInformation("Fetching market data for zip code: {ZipCode}, homeType: {HomeType}, keywords: {Keywords}", zipCode, homeType, keywords);

        var cachedData = await _marketDataRepository.GetByZipCodeAndKeywordsAsync(zipCode, homeType, keywords);

        if (cachedData != null && !IsExpired(cachedData))
        {
            _logger.LogInformation("Using cached market data for zip code: {ZipCode}, homeType: {HomeType}, keywords: {Keywords}", zipCode, homeType, keywords);
            var cached = await _marketDataRepository.GetPropertiesAsync(zipCode, homeType, keywords);
            return cached.Select(MapToZillowProperty).ToList();
        }

        _logger.LogInformation("Cache miss or expired for zip code: {ZipCode}, homeType: {HomeType}, keywords: {Keywords}. Fetching from Zillow API.", zipCode, homeType, keywords);

        return await RefreshMarketDataAsync(zipCode, homeType, keywords);
    }

    public async Task<List<ZillowProperty>> RefreshMarketDataAsync(string zipCode, string homeType, string keywords = "")
    {
        _logger.LogInformation("Refreshing market data from Zillow API for zip code: {ZipCode}, homeType: {HomeType}, keywords: {Keywords}", zipCode, homeType, keywords);

        if (!Enum.TryParse<ZillowHomeType>(homeType, out var parsedHomeType))
        {
            _logger.LogWarning("Invalid home type: {HomeType}. Defaulting to Houses.", homeType);
            parsedHomeType = ZillowHomeType.Houses;
        }

        var searchRequest = new ZillowSearchRequest
        {
            Location = zipCode,
            StatusType = ZillowStatusType.RecentlySold,
            HomeType = parsedHomeType,
            Sort = ZillowSort.Newest,
            SoldInLast = "12m",
            Keywords = string.IsNullOrWhiteSpace(keywords) ? null : keywords
        };

        var response = await _zillowApiService.SearchPropertiesAsync(searchRequest);

        if (response?.Properties == null || response.Properties.Count == 0)
        {
            _logger.LogWarning("No properties found for zip code: {ZipCode}, homeType: {HomeType}, keywords: {Keywords}", zipCode, homeType, keywords);
            return new List<ZillowProperty>();
        }

        _logger.LogInformation("Found {Count} properties for zip code: {ZipCode}, homeType: {HomeType}, keywords: {Keywords}", response.Properties.Count, zipCode, homeType, keywords);

        var marketData = new MarketData
        {
            ZipCode = zipCode,
            HomeType = homeType,
            Keywords = keywords,
            Source = "Zillow",
            FetchedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        var cachedProperties = response.Properties.Select(MapToCachedProperty).ToList();

        await _marketDataRepository.UpsertAsync(marketData, cachedProperties);

        return response.Properties;
    }

    public async Task<bool> HasFreshDataAsync(string zipCode, string homeType, string keywords = "")
    {
        return await _marketDataRepository.IsFreshDataAvailableAsync(zipCode, homeType, keywords);
    }

    private bool IsExpired(MarketData marketData)
    {
        if (marketData.ExpiresAt == null) return false;
        return marketData.ExpiresAt.Value < DateTime.UtcNow;
    }

    private static CachedProperty MapToCachedProperty(ZillowProperty p) => new()
    {
        Zpid = p.Id,
        PropertyType = p.PropertyType,
        Address = p.Address,
        City = p.City,
        State = p.State,
        ZipCode = p.ZipCode,
        Latitude = p.Latitude,
        Longitude = p.Longitude,
        Bedrooms = p.Bedrooms,
        Bathrooms = p.Bathrooms,
        LivingArea = p.LivingArea,
        DetailUrl = p.DetailUrl,
        ListingStatus = p.ListingStatus,
        Zestimate = p.Zestimate,
        DaysOnZillow = p.DaysOnZillow,
        Price = p.Price,
        DateSoldTimestamp = p.DateSoldTimestamp
    };

    private static ZillowProperty MapToZillowProperty(CachedProperty p) => new()
    {
        Id = p.Zpid,
        PropertyType = p.PropertyType,
        Address = p.Address,
        City = p.City,
        State = p.State,
        ZipCode = p.ZipCode,
        Latitude = p.Latitude,
        Longitude = p.Longitude,
        Bedrooms = p.Bedrooms,
        Bathrooms = p.Bathrooms,
        LivingArea = p.LivingArea,
        DetailUrl = p.DetailUrl,
        ListingStatus = p.ListingStatus,
        Zestimate = p.Zestimate,
        DaysOnZillow = p.DaysOnZillow,
        Price = p.Price,
        DateSoldTimestamp = p.DateSoldTimestamp
    };
}
