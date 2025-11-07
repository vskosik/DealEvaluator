using DealEvaluator.Application.DTOs.Zillow;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DealEvaluator.Application.Services;

/// <summary>
/// Service for automatically finding comparable properties from market data
/// </summary>
public class CompService : ICompService
{
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<CompService> _logger;

    public CompService(
        IMarketDataService marketDataService,
        ILogger<CompService> logger)
    {
        _marketDataService = marketDataService;
        _logger = logger;
    }

    public async Task<List<ZillowProperty>> FindComparablesAsync(
        PropertyTypes propertyType,
        int? bedrooms,
        int? bathrooms,
        int? sqft,
        string zipCode,
        string? subjectPropertyAddress = null)
    {
        _logger.LogInformation(
            "Finding comparables for {PropertyType} with {Beds}bd/{Baths}ba, {Sqft}sqft in {ZipCode}, excluding address: {subjectPropertyAddress}",
            propertyType, bedrooms, bathrooms, sqft, zipCode, subjectPropertyAddress);

        // Get market data for the zip code and ARV keywords
        var marketData = await _marketDataService.GetMarketDataForZipCodeAsync(zipCode, "renovated");

        if (marketData == null || marketData.Count == 0)
        {
            throw new InvalidOperationException($"No market data available for zip code {zipCode}");
        }

        // Filter by property type (must match)
        var propertyTypeFiltered = FilterByPropertyType(marketData, propertyType);

        // Exclude subject property if address provided
        if (!string.IsNullOrWhiteSpace(subjectPropertyAddress))
        {
            propertyTypeFiltered = propertyTypeFiltered.Where(p => 
                    !string.Equals(p.Address, subjectPropertyAddress, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        
        if (propertyTypeFiltered.Count == 0)
        {
            throw new InvalidOperationException(
                $"No properties of type {propertyType} found in market data for zip code {zipCode}");
        }

        _logger.LogInformation("Found {Count} properties of type {PropertyType}",
            propertyTypeFiltered.Count, propertyType);

        // Progressive search with criteria widening
        List<ZillowProperty> comparables;

        // Try 1: Exact bed/bath match with ±10% sqft
        comparables = FindMatchingProperties(propertyTypeFiltered, bedrooms, bathrooms, sqft, 0, 0.10m);
        if (comparables.Count >= 3)
        {
            _logger.LogInformation("Found {Count} exact matches (±10% sqft)", comparables.Count);
            return comparables.Take(5).ToList(); // Return up to 5 for exact matches
        }

        // Try 2: ±1 bed/bath with ±10% sqft
        comparables = FindMatchingProperties(propertyTypeFiltered, bedrooms, bathrooms, sqft, 1, 0.10m);
        if (comparables.Count >= 3)
        {
            _logger.LogInformation("Found {Count} comparables with ±1 bed/bath, ±10% sqft", comparables.Count);
            return comparables.Take(3).ToList(); // Return up to 3 for widened search
        }

        // Try 3: ±1 bed/bath with ±20% sqft
        comparables = FindMatchingProperties(propertyTypeFiltered, bedrooms, bathrooms, sqft, 1, 0.20m);
        if (comparables.Count >= 3)
        {
            _logger.LogInformation("Found {Count} comparables with ±1 bed/bath, ±20% sqft", comparables.Count);
            return comparables.Take(3).ToList();
        }

        // Try 4: ±1 bed/bath with ±30% sqft
        comparables = FindMatchingProperties(propertyTypeFiltered, bedrooms, bathrooms, sqft, 1, 0.30m);
        if (comparables.Count >= 3)
        {
            _logger.LogInformation("Found {Count} comparables with ±1 bed/bath, ±30% sqft", comparables.Count);
            return comparables.Take(3).ToList();
        }

        // Failed to find at least 3 comparables
        throw new InvalidOperationException(
            $"Could not find at least 3 comparable properties for {propertyType} with {bedrooms}bd/{bathrooms}ba, {sqft}sqft in {zipCode}. " +
            $"Only found {comparables.Count} properties after widening criteria.");
    }

    /// <summary>
    /// Finds properties matching bed/bath/sqft criteria with specified tolerances
    /// </summary>
    private List<ZillowProperty> FindMatchingProperties(
        List<ZillowProperty> properties,
        int? targetBedrooms,
        int? targetBathrooms,
        int? targetSqft,
        int bedBathTolerance,
        decimal sqftTolerancePercent)
    {
        var matches = properties.Where(p =>
        {
            // Bed match
            bool bedMatch = !targetBedrooms.HasValue || !p.Bedrooms.HasValue ||
                            Math.Abs(p.Bedrooms.Value - targetBedrooms.Value) <= bedBathTolerance;

            // Bath match (using float for Zillow bathrooms)
            bool bathMatch = !targetBathrooms.HasValue || !p.Bathrooms.HasValue ||
                            Math.Abs(p.Bathrooms.Value - targetBathrooms.Value) <= bedBathTolerance;

            // Sqft match
            bool sqftMatch = true;
            if (targetSqft.HasValue && p.LivingArea.HasValue)
            {
                var tolerance = (int)(targetSqft.Value * sqftTolerancePercent);
                var minSqft = targetSqft.Value - tolerance;
                var maxSqft = targetSqft.Value + tolerance;
                sqftMatch = p.LivingArea.Value >= minSqft && p.LivingArea.Value <= maxSqft;
            }

            // Must have a price to be a valid comparable
            bool hasPrice = p.Price.HasValue && p.Price.Value > 0;

            return bedMatch && bathMatch && sqftMatch && hasPrice;
        }).ToList();

        // Sort by sqft proximity (closest match first) if target sqft is available
        if (targetSqft.HasValue)
        {
            matches = matches
                .OrderBy(p => p.LivingArea.HasValue
                    ? Math.Abs(p.LivingArea.Value - targetSqft.Value)
                    : int.MaxValue)
                .ToList();
        }

        return matches;
    }

    /// <summary>
    /// Filters properties by property type
    /// </summary>
    private List<ZillowProperty> FilterByPropertyType(List<ZillowProperty> properties, PropertyTypes propertyType)
    {
        var targetType = MapPropertyTypeToZillowType(propertyType);
        return properties
            .Where(p => p.PropertyType != null &&
                        p.PropertyType.Equals(targetType, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Maps our PropertyTypes enum to Zillow property type strings
    /// </summary>
    private string MapPropertyTypeToZillowType(PropertyTypes propertyType)
    {
        return propertyType switch
        {
            PropertyTypes.SingleFamily => "SINGLE_FAMILY",
            PropertyTypes.MultiFamily => "MULTI_FAMILY",
            PropertyTypes.Condo => "CONDO",
            PropertyTypes.Townhouse => "TOWNHOUSE",
            _ => throw new ArgumentException($"Unknown property type: {propertyType}")
        };
    }
}