using DealEvaluator.Application.DTOs.Zillow;

namespace DealEvaluator.Application.Interfaces;

public interface IMarketDataService
{
    /// <summary>
    /// Gets market data for a zip code with optional keywords. Checks cache first, then calls Zillow API if needed.
    /// </summary>
    /// <param name="zipCode">The zip code to fetch data for</param>
    /// <param name="homeType">The type of home to search for (e.g., "Houses", "MultiFamily")</param>
    /// <param name="keywords">Optional keywords to filter properties (e.g., "renovated,rehabbed,brand new")</param>
    /// <returns>List of properties available in the zip code</returns>
    Task<List<ZillowProperty>> GetMarketDataForZipCodeAsync(string zipCode, string homeType, string keywords = "");

    /// <summary>
    /// Forces a refresh of market data from Zillow API regardless of cache status
    /// </summary>
    /// <param name="zipCode">The zip code to fetch data for</param>
    /// <param name="homeType">The type of home to search for (e.g., "Houses", "MultiFamily")</param>
    /// <param name="keywords">Optional keywords to filter properties (e.g., "renovated,rehabbed,brand new")</param>
    Task<List<ZillowProperty>> RefreshMarketDataAsync(string zipCode, string homeType, string keywords = "");

    /// <summary>
    /// Checks if fresh cached data is available for a zip code and keywords combination
    /// </summary>
    /// <param name="zipCode">The zip code to check</param>
    /// <param name="homeType">The type of home to check for</param>
    /// <param name="keywords">Optional keywords</param>
    Task<bool> HasFreshDataAsync(string zipCode, string homeType, string keywords = "");
}