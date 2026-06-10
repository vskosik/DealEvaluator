using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Interfaces;

/// <summary>
/// Service for automatically finding comparable properties from market data
/// </summary>
public interface ICompService
{
    /// <summary>
    /// Finds up to 5 comparable properties using weighted similarity scoring
    /// (sqft/beds/baths) with hard gates and $/sqft outlier trimming.
    /// Never throws for "not enough comps" — returns a result graded
    /// High/Medium/Low/Insufficient so callers decide how to proceed.
    /// </summary>
    /// <param name="propertyType">Property type to match</param>
    /// <param name="bedrooms">Number of bedrooms</param>
    /// <param name="bathrooms">Number of bathrooms (fractional, e.g. 2.5)</param>
    /// <param name="sqft">Square footage</param>
    /// <param name="zipCode">Zip code to search in</param>
    /// <param name="subjectPropertyAddress">Address of the subject property to exclude from results</param>
    /// <param name="subjectZpid">Zillow property ID of the subject; when provided, used instead of address comparison</param>
    /// <param name="searchKeyword">Optional keyword to filter the market data fetch (e.g. "renovated").
    /// Defaults to empty — fetch all recently-sold properties of the given type rather than
    /// narrowing to listings that mention the keyword in their description.</param>
    /// <returns>Scored comparables plus confidence grade and diagnostics</returns>
    Task<CompSearchResult> FindComparablesAsync(
        PropertyTypes propertyType,
        int? bedrooms,
        decimal? bathrooms,
        int? sqft,
        string zipCode,
        string? subjectPropertyAddress = null,
        string? subjectZpid = null,
        double? subjectLatitude = null,
        double? subjectLongitude = null,
        string searchKeyword = "");
}