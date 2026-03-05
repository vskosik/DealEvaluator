using System.Net;
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
    private readonly int _baseDelayMs;
    private readonly int _maxRetries;
    private readonly int _initialBackoffMs;
    private readonly int _maxBackoffMs;
    private readonly int _jitterMs;
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
        _baseDelayMs = GetConfigInt(configuration, "ZillowApiRateLimit:BaseDelayMs", 1000);
        _maxRetries = GetConfigInt(configuration, "ZillowApiRateLimit:MaxRetries", 4);
        _initialBackoffMs = GetConfigInt(configuration, "ZillowApiRateLimit:InitialBackoffMs", 2000);
        _maxBackoffMs = GetConfigInt(configuration, "ZillowApiRateLimit:MaxBackoffMs", 20000);
        _jitterMs = GetConfigInt(configuration, "ZillowApiRateLimit:JitterMs", 300);
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

            if (currentPage <= totalPages)
            {
                _logger.LogDebug("Applying page delay of {DelayMs}ms before next Zillow request", _baseDelayMs);
                await Task.Delay(_baseDelayMs);
            }
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

        return await SendWithRetryAsync(httpRequest, request.Page);
    }

    private async Task<ZillowSearchResponse> SendWithRetryAsync(HttpRequestMessage initialRequest, int page)
    {
        var random = new Random();

        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            using var request = CloneRequest(initialRequest);
            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ZillowSearchResponse>(json, JsonOptions)!;
            }

            var statusCode = response.StatusCode;
            var shouldRetry = IsRetryableStatusCode(statusCode);

            if (!shouldRetry || attempt == _maxRetries)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Zillow request failed for page {page} with status {(int)statusCode} {statusCode}. Body: {responseText}",
                    null,
                    statusCode);
            }

            var retryDelay = GetRetryDelay(response, attempt, random);
            _logger.LogWarning(
                "Zillow request throttled/failed for page {Page}. Status: {StatusCode}. Attempt {Attempt}/{MaxRetries}. Retrying in {DelayMs}ms",
                page, (int)statusCode, attempt + 1, _maxRetries, retryDelay.TotalMilliseconds);

            await Task.Delay(retryDelay);
        }

        throw new InvalidOperationException("Retry loop exited unexpectedly.");
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage source)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri);

        foreach (var header in source.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    private static bool IsRetryableStatusCode(HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return true;
        }

        var numericStatusCode = (int)statusCode;
        return numericStatusCode >= 500 && numericStatusCode < 600;
    }

    private TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt, Random random)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta.HasValue == true)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter?.Date.HasValue == true)
        {
            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return delay;
            }
        }

        var exponentialBackoffMs = Math.Min(_maxBackoffMs, _initialBackoffMs * (int)Math.Pow(2, attempt));
        var jitter = random.Next(0, Math.Max(1, _jitterMs));
        return TimeSpan.FromMilliseconds(exponentialBackoffMs + jitter);
    }

    private static int GetConfigInt(IConfiguration configuration, string key, int fallback)
    {
        var raw = configuration[key];
        return int.TryParse(raw, out var parsed) && parsed >= 0 ? parsed : fallback;
    }
}
