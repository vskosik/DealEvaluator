using DealEvaluator.Application.DTOs.Zillow;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Interfaces;

/// <summary>
/// Service for automatically finding comparable properties from market data
/// </summary>
public interface ICompService
{
    /// <summary>
    /// Finds 3-5 comparable properties using progressive search criteria widening.
    /// Returns up to 5 comps for exact matches, up to 3 for widened searches.
    /// </summary>
    /// <param name="propertyType">Property type to match</param>
    /// <param name="bedrooms">Number of bedrooms</param>
    /// <param name="bathrooms">Number of bathrooms</param>
    /// <param name="sqft">Square footage</param>
    /// <param name="zipCode">Zip code to search in</param>
    /// <returns>List of comparable properties</returns>
    /// <exception cref="InvalidOperationException">If less than 3 comparables found after all criteria widening</exception>
    Task<List<ZillowProperty>> FindComparablesAsync(
        PropertyTypes propertyType,
        int? bedrooms,
        int? bathrooms,
        int? sqft,
        string zipCode);
}