using DealEvaluator.Application.DTOs.Zillow;

namespace DealEvaluator.Application.Interfaces;

public interface IMarketDataService
{
    /// <summary>
    /// Gets market data for a zip code. Checks cache first, then calls Zillow API if needed.
    /// </summary>
    /// <param name="zipCode">The zip code to fetch data for</param>
    /// <returns>List of properties available in the zip code</returns>
    Task<List<ZillowProperty>> GetMarketDataForZipCodeAsync(string zipCode);

    /// <summary>
    /// Forces a refresh of market data from Zillow API regardless of cache status
    /// </summary>
    Task<List<ZillowProperty>> RefreshMarketDataAsync(string zipCode);

    /// <summary>
    /// Checks if fresh cached data is available for a zip code
    /// </summary>
    Task<bool> HasFreshDataAsync(string zipCode);
}