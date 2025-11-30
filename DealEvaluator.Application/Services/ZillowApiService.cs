using System.Text.Json;
using System.Text.Json.Serialization;
using DealEvaluator.Application.DTOs.Zillow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DealEvaluator.Application.Services;

public class ZillowApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZillowApiService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _rapidApiHost;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IgnoreReadOnlyProperties = true,
        AllowTrailingCommas = true
    };

    public ZillowApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ZillowApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _apiKey = configuration["RapidApiKey"]
                  ?? throw new InvalidOperationException("RapidApiKey is missing in configuration");

        _rapidApiHost = configuration["ZillowRapidApiHost"]
                         ?? throw new InvalidOperationException("ZillowRapidApiHost is missing in configuration");

        _baseUrl = "https://" + _rapidApiHost;
    }

    public async Task<ZillowSearchResponse> SearchPropertiesAsync(ZillowSearchRequest request)
    {
        _logger.LogInformation("Starting property search for location: {Location}, homeType: {HomeType}",
            request.Location, request.HomeType);

        var allProperties = new List<ZillowProperty>();
        int currentPage = 1;
        int totalPages = 1;
        int totalResults = 0;
        int resultsPerPage = 0;

        do
        {
            request.Page = currentPage;
            var pageResponse = await FetchPageAsync(request);

            if (pageResponse?.Properties != null && pageResponse.Properties.Count > 0)
            {
                allProperties.AddRange(pageResponse.Properties);
                totalPages = pageResponse.TotalPages;
                totalResults = pageResponse.TotalResults;
                resultsPerPage = pageResponse.ResultsPerPage;

                _logger.LogInformation(
                    "Fetched page {CurrentPage}/{TotalPages} with {Count} properties (Total: {Total})",
                    currentPage, totalPages, pageResponse.Properties.Count, totalResults);
            }
            else
            {
                _logger.LogWarning("Page {Page} returned no properties, stopping pagination", currentPage);
                break;
            }

            currentPage++;
        } while (currentPage <= totalPages);

        _logger.LogInformation(
            "Completed property search. Total properties fetched: {Count}/{Total} across {Pages} pages",
            allProperties.Count, totalResults, currentPage - 1);

        return new ZillowSearchResponse
        {
            Properties = allProperties,
            TotalResults = totalResults,
            ResultsPerPage = resultsPerPage,
            TotalPages = totalPages,
            CurrentPage = currentPage - 1 // Last page fetched
        };
    }

    private async Task<ZillowSearchResponse> FetchPageAsync(ZillowSearchRequest request)
    {
        var queryParams = new List<string>
        {
            $"location={request.Location}",
            $"sort={request.Sort}",
            $"home_type={request.HomeType}",
            $"status_type={request.StatusType}",
            $"page={request.Page}"
        };

        if (request.BathsMin.HasValue) queryParams.Add($"bathsMin={request.BathsMin.Value}");
        if (request.BathsMax.HasValue) queryParams.Add($"bathsMax={request.BathsMax.Value}");
        if (request.BedsMin.HasValue) queryParams.Add($"bedsMin={request.BedsMin.Value}");
        if (request.BedsMax.HasValue) queryParams.Add($"bedsMax={request.BedsMax.Value}");

        if (request.StatusType == ZillowStatusType.RecentlySold)
        {
            if (!string.IsNullOrEmpty(request.SoldInLast)) queryParams.Add($"soldInLast={request.SoldInLast}");
        }
        else
        {
            if (!string.IsNullOrEmpty(request.DaysOn)) queryParams.Add($"daysOn={request.DaysOn}");
        }

        if (request.IsBasementFinished.HasValue) queryParams.Add($"isBasementFinished={request.IsBasementFinished.Value}");
        if (request.IsBasementUnfinished.HasValue) queryParams.Add($"isBasementUnfinished={request.IsBasementUnfinished.Value}");
        if (request.IsPendingUnderContract.HasValue) queryParams.Add($"isPendingUnderContract={request.IsPendingUnderContract.Value}");
        if (!string.IsNullOrEmpty(request.Keywords)) queryParams.Add($"keywords={request.Keywords}");

        var url = $"{_baseUrl}/propertyExtendedSearch?{string.Join("&", queryParams)}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Add("x-rapidapi-key", _apiKey);
        httpRequest.Headers.Add("x-rapidapi-host", _rapidApiHost);

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ZillowSearchResponse>(json, JsonOptions)!;
    }
}