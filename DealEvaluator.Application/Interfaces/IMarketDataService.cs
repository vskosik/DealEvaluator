using DealEvaluator.Application.DTOs.Zillow;

namespace DealEvaluator.Application.Interfaces;

public interface IMarketDataService
{
    /// <summary>
    /// Gets market data for a zip code with optional keywords. Checks cache first, then calls Zillow API if needed.
    /// </summary>
    /// <param name="zipCode">The zip code to fetch data for</param>
    /// <param name="keywords">Optional keywords to filter properties (e.g., "renovated,rehabbed,brand new")</param>
    /// <returns>List of properties available in the zip code</returns>
    Task<List<ZillowProperty>> GetMarketDataForZipCodeAsync(string zipCode, string keywords = "");

    /// <summary>
    /// Forces a refresh of market data from Zillow API regardless of cache status
    /// </summary>
    /// <param name="zipCode">The zip code to fetch data for</param>
    /// <param name="keywords">Optional keywords to filter properties (e.g., "renovated,rehabbed,brand new")</param>
    Task<List<ZillowProperty>> RefreshMarketDataAsync(string zipCode, string keywords = "");

    /// <summary>
    /// Checks if fresh cached data is available for a zip code and keywords combination
    /// </summary>
    Task<bool> HasFreshDataAsync(string zipCode, string keywords = "");
}