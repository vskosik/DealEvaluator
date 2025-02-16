using System.Text.Json;
using Deal_Evaluator.DTOs.Zillow;

namespace Deal_Evaluator.API;

public class ZillowApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public ZillowApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        //_apiKey = configuration["ZILLOW_API_KEY"];
        _apiKey = Environment.GetEnvironmentVariable("RAPID_API_KEY");
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "zillow-com1.p.rapidapi.com");
        _baseUrl = configuration["BaseURL"]; // TODO: Configure real base url
    }

    public async Task<ZillowSearchResponse> SearchPropertiesAsync(ZillowSearchRequest request)
    {
        var queryParams = new List<string>
        {
            $"location={request.Location}",
            $"sort={request.Sort}",
            $"home_type={request.HomeType}",
            $"status_type={request.StatusType}"
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
        
        var url  = $"{_baseUrl}/propertyExtendedSearch?{string.Join("&", queryParams)}";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<ZillowSearchResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}