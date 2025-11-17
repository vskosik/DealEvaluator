using AutoMapper;
using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Application.DTOs.Evaluation;
using DealEvaluator.Application.DTOs.Property;
using DealEvaluator.Application.DTOs.Rehab;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using System.Text.Json;
using DealEvaluator.Domain.Enums;

namespace DealEvaluator.Application.Services;

public class PropertyService : IPropertyService
{
    // Cost constants for realistic profit calculation
    // These can be made configurable in future features
    private const decimal SellingAgentCommission = 0.06m;    // 6% of ARV
    private const decimal SellingClosingCosts = 0.02m;       // 2% of ARV
    private const decimal BuyingClosingCosts = 0.02m;        // 2% of purchase price
    private const decimal AnnualPropertyTaxRate = 0.012m;    // 1.2% annually
    private const decimal MonthlyInsurance = 150m;           // Monthly insurance cost
    private const decimal MonthlyUtilities = 200m;           // Monthly utilities cost
    private const int DefaultHoldingMonths = 4;              // Average rehab holding period

    private readonly IPropertyRepository _propertyRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly ICompService _compService;
    private readonly IRehabCostTemplateRepository _rehabTemplateRepository;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;

    public PropertyService(
        IPropertyRepository propertyRepository,
        IEvaluationRepository evaluationRepository,
        ICompService compService,
        IRehabCostTemplateRepository rehabTemplateRepository,
        IMapper mapper,
        IHttpClientFactory httpClientFactory)
    {
        _propertyRepository = propertyRepository;
        _evaluationRepository = evaluationRepository;
        _compService = compService;
        _rehabTemplateRepository = rehabTemplateRepository;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PropertyDto> CreatePropertyAsync(CreatePropertyDto dto, string userId)
    {
        // Check if property already exists for this user at this address
        var existingProperty = await _propertyRepository
            .GetPropertyByAddressAndUserAsync(dto.Address, userId);

        if (existingProperty != null)
        {
            // Update existing property with new values
            _mapper.Map(dto, existingProperty);

            _propertyRepository.Update(existingProperty);
            await _propertyRepository.SaveChangesAsync();

            return _mapper.Map<PropertyDto>(existingProperty);
        }

        // Create new property
        var property = _mapper.Map<Property>(dto);
        property.UserId = userId;
        property.CreatedAt = DateTime.UtcNow;

        // Geocode the address to get coordinates
        var (latitude, longitude) = await GeocodeAddressAsync(dto.Address, dto.City, dto.State);
        property.Latitude = latitude;
        property.Longitude = longitude;

        await _propertyRepository.AddAsync(property);
        await _propertyRepository.SaveChangesAsync();

        // Try to create automatic evaluation
        try
        {
            var propertyAddress = $"{property.Address}, {property.City}, {property.State} {property.ZipCode}";

            // Try to find comparables automatically
            var zillowComps = await _compService.FindComparablesAsync(
                property.PropertyType,
                property.Bedrooms,
                property.Bathrooms,
                property.Sqft,
                property.ZipCode,
                propertyAddress);

            // Save comparables first to get their IDs
            var comparableIds = new List<int>();
            foreach (var zillowComp in zillowComps)
            {
                var addressParts = ParseZillowAddress(zillowComp.Address);

                var comparable = new Comparable
                {
                    PropertyId = property.Id,
                    Address = addressParts.Street,
                    City = addressParts.City,
                    State = addressParts.State,
                    ZipCode = addressParts.ZipCode,
                    Price = zillowComp.Price,
                    Sqft = zillowComp.LivingArea,
                    Bedrooms = zillowComp.Bedrooms,
                    Bathrooms = (int?)zillowComp.Bathrooms,
                    Latitude = zillowComp.Latitude,
                    Longitude = zillowComp.Longitude,
                    Source = "Zillow",
                    ComparableType = ComparableType.Arv
                };

                var savedComparable = await _propertyRepository.CreateComparableAsync(comparable);
                comparableIds.Add(savedComparable.Id);
            }

            // Generate rehab line items - use user input if provided, otherwise auto-generate
            List<CreateRehabLineItemDto> rehabLineItemDtos;

            if (dto.RepairCost.HasValue && dto.RepairCost.Value > 0)
            {
                // User provided their own repair cost - create a single "Other" line item
                rehabLineItemDtos = new List<CreateRehabLineItemDto>
                {
                    new()
                    {
                        LineItemType = RehabLineItemType.Other,
                        Condition = MapPropertyConditionToRehabCondition(property.PropertyConditions),
                        Quantity = 1,
                        UnitCost = dto.RepairCost.Value,
                        Notes = "User-provided repair cost estimate"
                    }
                };
            }
            else
            {
                // Auto-generate based on property details
                rehabLineItemDtos = await GenerateAutoRehabLineItemDtosAsync(property, userId);
            }

            // Create evaluation using the existing method
            var createEvaluationDto = new CreateEvaluationDto
            {
                PropertyId = property.Id,
                ComparableIds = comparableIds,
                RehabLineItems = rehabLineItemDtos
            };

            await CreateEvaluationAsync(createEvaluationDto);
        }
        catch (InvalidOperationException)
        {
            // If we can't find enough comparables, don't create an evaluation
            // User can create one manually later
            // Silently continue - property was still created successfully
        }

        return _mapper.Map<PropertyDto>(property);
    }

    public async Task<PropertyDto?> GetPropertyByIdAsync(int id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        return _mapper.Map<PropertyDto>(property);
    }

    public async Task<List<PropertyDto>> GetUserPropertiesAsync(string userId)
    {
        var properties = await _propertyRepository.GetPropertiesByUserIdAsync(userId);
        
        return _mapper.Map<List<PropertyDto>>(properties);
    }

    public async Task<PropertyDto> UpdatePropertyAsync(UpdatePropertyDto dto)
    {
        // Get existing property
        var property = await _propertyRepository.GetByIdAsync(dto.Id);
        if (property == null)
            throw new KeyNotFoundException($"Property with ID {dto.Id} not found");

        // Map updates to entity
        _mapper.Map(dto, property);

        // Save changes
        _propertyRepository.Update(property);
        await _propertyRepository.SaveChangesAsync();

        return _mapper.Map<PropertyDto>(property);
    }

    public async Task DeletePropertyAsync(int id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null)
            throw new KeyNotFoundException($"Property with ID {id} not found");

        _propertyRepository.Delete(property);
        await _propertyRepository.SaveChangesAsync();
    }

    public async Task<List<EvaluationDto>> GetPropertyEvaluationsAsync(int propertyId)
    {
        var evaluations = await _evaluationRepository.GetEvaluationsByPropertyIdAsync(propertyId);
        return _mapper.Map<List<EvaluationDto>>(evaluations);
    }

    public async Task<EvaluationDto?> GetLatestEvaluationAsync(int propertyId)
    {
        var evaluation = await _evaluationRepository.GetLatestEvaluationByPropertyIdAsync(propertyId);
        return _mapper.Map<EvaluationDto?>(evaluation);
    }

    public async Task<List<ComparableDto>> GetComparablesAsync(int propertyId)
    {
        var comparables = await _propertyRepository.GetComparablesAsync(propertyId);
        return _mapper.Map<List<ComparableDto>>(comparables);
    }

    public async Task DeleteComparableAsync(int comparableId)
    {
        await _propertyRepository.DeleteComparableAsync(comparableId);
    }

    public async Task<ComparableDto> CreateComparableFromMarketDataAsync(CreateComparableDto dto)
    {
        var comparable = _mapper.Map<Comparable>(dto);
        var createdComparable = await _propertyRepository.CreateComparableAsync(comparable);
        return _mapper.Map<ComparableDto>(createdComparable);
    }

    public async Task<EvaluationDto> CreateEvaluationAsync(CreateEvaluationDto dto)
    {
        // Validate property exists
        var property = await _propertyRepository.GetByIdAsync(dto.PropertyId);
        if (property == null)
            throw new KeyNotFoundException($"Property with ID {dto.PropertyId} not found");

        // Fetch comparables by IDs
        var comparables = new List<Comparable>();
        foreach (var compId in dto.ComparableIds)
        {
            var comp = await _propertyRepository.GetComparableByIdAsync(compId);
            if (comp != null)
                comparables.Add(comp);
        }

        if (comparables.Count == 0)
            throw new InvalidOperationException("At least one valid comparable is required to create an evaluation");

        // Calculate ARV (average price of comparables)
        var comparablePrices = comparables
            .Where(c => c.Price.HasValue && c.Price.Value > 0)
            .Select(c => c.Price!.Value)
            .ToList();

        if (comparablePrices.Count == 0)
            throw new InvalidOperationException("Comparables must have valid prices to calculate ARV");

        int arv = (int)comparablePrices.Average();

        // Create rehab estimate and line items
        var rehabEstimate = new RehabEstimate
        {
            CreatedAt = DateTime.UtcNow,
            LineItems = dto.RehabLineItems.Select(li => new RehabLineItem
            {
                LineItemType = li.LineItemType,
                Condition = li.Condition,
                Quantity = li.Quantity,
                UnitCost = li.UnitCost,
                Notes = li.Notes
            }).ToList()
        };

        // Calculate repair cost from rehab estimate (TotalCost is computed property)
        int repairCost = (int)Math.Round(rehabEstimate.TotalCost);

        // Calculate 70% Rule max offer (traditional formula)
        int maxOffer = (int)(arv * 0.7m) - repairCost;

        // Calculate realistic profit with all costs included
        // Selling costs (paid when selling the property)
        decimal sellingCosts = arv * (SellingAgentCommission + SellingClosingCosts);

        // Buying costs (paid when purchasing)
        decimal buyingCosts = maxOffer * BuyingClosingCosts;

        // Holding costs (monthly expenses during rehab period)
        decimal monthlyPropertyTax = (arv * AnnualPropertyTaxRate) / 12;
        decimal monthlyHoldingCosts = monthlyPropertyTax + MonthlyInsurance + MonthlyUtilities;
        decimal totalHoldingCosts = monthlyHoldingCosts * DefaultHoldingMonths;

        // Total investment (all money you put in)
        decimal totalInvestment = maxOffer + buyingCosts + repairCost + totalHoldingCosts;

        // Net proceeds from sale (what you get after selling)
        decimal netProceeds = arv - sellingCosts;

        // Real profit and ROI
        int profit = (int)Math.Round(netProceeds - totalInvestment);
        decimal? roi = totalInvestment > 0 ? (netProceeds - totalInvestment) / totalInvestment * 100 : null;

        // Create evaluation entity
        var evaluation = new Evaluation
        {
            PropertyId = dto.PropertyId,
            Arv = arv,
            MaxOffer = maxOffer,
            Profit = profit,
            Roi = roi,
            CreatedAt = DateTime.UtcNow,
            Comparables = comparables,
            RehabEstimate = rehabEstimate
        };

        // Save evaluation
        await _evaluationRepository.AddAsync(evaluation);
        await _evaluationRepository.SaveChangesAsync();

        // Return DTO
        return _mapper.Map<EvaluationDto>(evaluation);
    }

    /// <summary>
    /// Geocodes an address using OpenStreetMap's Nominatim service
    /// Returns coordinates (latitude, longitude) or null if geocoding fails
    /// </summary>
    private async Task<(double? Latitude, double? Longitude)> GeocodeAddressAsync(string address, string city, string state)
    {
        try
        {
            var fullAddress = $"{address}, {city}, {state}";
            var geocodeUrl = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(fullAddress)}";

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DealEvaluator/1.0"); // API requires user agent

            var response = await httpClient.GetAsync(geocodeUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(json);

            if (results is not { Count: > 0 }) 
                return (null, null);
            
            var result = results[0];
            if (double.TryParse(result.Lat, out var lat) && double.TryParse(result.Lon, out var lon))
            {
                return (lat, lon);
            }

            return (null, null);
        }
        catch (Exception)
        {
            return (null, null);
        }
    }

    /// <summary>
    /// Parses Zillow's combined address format: "{street}, {city}, {state} {zip}"
    /// Example: "9218 Success Ave, Los Angeles, CA 90002"
    /// </summary>
    private static (string Street, string City, string State, string ZipCode) ParseZillowAddress(string? fullAddress)
    {
        if (string.IsNullOrWhiteSpace(fullAddress))
            return ("", "", "", "");

        var parts = fullAddress.Split(',').Select(p => p.Trim()).ToArray();

        if (parts.Length < 3) 
            return (Street: fullAddress, City: "", State: "", ZipCode: "");
        
        // Last part should have "STATE ZIP" format
        var lastPart = parts[^1];
        var stateZipMatch = System.Text.RegularExpressions.Regex.Match(lastPart, @"([A-Z]{2})\s+(\d{5})");

        if (stateZipMatch.Success)
        {
            return (
                Street: parts[0],
                City: parts[1],
                State: stateZipMatch.Groups[1].Value,
                ZipCode: stateZipMatch.Groups[2].Value
            );
        }
        
        return (Street: fullAddress, City: "", State: "", ZipCode: "");
    }

    // DTO for Nominatim API response
    private class NominatimResult
    {
        public string Lat { get; set; } = string.Empty;
        public string Lon { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maps PropertyConditions to RehabCondition for auto-generating rehab estimates
    /// </summary>
    private static RehabCondition MapPropertyConditionToRehabCondition(PropertyConditions propertyCondition)
    {
        return propertyCondition switch
        {
            PropertyConditions.Excellent => RehabCondition.Cosmetic,    // Light refresh
            PropertyConditions.MinorRepairs => RehabCondition.Moderate, // Some work needed
            PropertyConditions.Outdated => RehabCondition.Moderate,     // Needs updates
            PropertyConditions.Bad => RehabCondition.Heavy,             // Major renovation
            PropertyConditions.Horrible => RehabCondition.Heavy,        // Full gut
            _ => RehabCondition.Moderate                                 // Default to moderate
        };
    }

    /// <summary>
    /// Auto-generates rehab line item DTOs based on property details
    /// Uses the same logic as the frontend auto-prefill
    /// </summary>
    private async Task<List<CreateRehabLineItemDto>> GenerateAutoRehabLineItemDtosAsync(Property property, string userId)
    {
        var rehabCondition = MapPropertyConditionToRehabCondition(property.PropertyConditions);
        var lineItemDtos = new List<CreateRehabLineItemDto>();

        // Add Bedroom line items
        if (property.Bedrooms.HasValue && property.Bedrooms.Value > 0)
        {
            var template = await _rehabTemplateRepository.GetTemplateByUserAndTypeConditionAsync(
                userId, RehabLineItemType.Bedroom, rehabCondition);

            var unitCost = template?.DefaultCost ?? GetDefaultCostFallback(RehabLineItemType.Bedroom, rehabCondition);

            lineItemDtos.Add(new CreateRehabLineItemDto
            {
                LineItemType = RehabLineItemType.Bedroom,
                Condition = rehabCondition,
                Quantity = property.Bedrooms.Value,
                UnitCost = unitCost,
                Notes = "Auto-generated based on property details"
            });
        }

        // Add Bathroom line items
        if (property.Bathrooms.HasValue && property.Bathrooms.Value > 0)
        {
            var template = await _rehabTemplateRepository.GetTemplateByUserAndTypeConditionAsync(
                userId, RehabLineItemType.Bathroom, rehabCondition);

            var unitCost = template?.DefaultCost ?? GetDefaultCostFallback(RehabLineItemType.Bathroom, rehabCondition);

            lineItemDtos.Add(new CreateRehabLineItemDto
            {
                LineItemType = RehabLineItemType.Bathroom,
                Condition = rehabCondition,
                Quantity = property.Bathrooms.Value,
                UnitCost = unitCost,
                Notes = "Auto-generated based on property details"
            });
        }

        // Add General line item (sqft-based)
        if (property.Sqft.HasValue && property.Sqft.Value > 0)
        {
            var template = await _rehabTemplateRepository.GetTemplateByUserAndTypeConditionAsync(
                userId, RehabLineItemType.General, rehabCondition);

            var unitCost = template?.DefaultCost ?? GetDefaultCostFallback(RehabLineItemType.General, rehabCondition);

            lineItemDtos.Add(new CreateRehabLineItemDto
            {
                LineItemType = RehabLineItemType.General,
                Condition = rehabCondition,
                Quantity = property.Sqft.Value,
                UnitCost = unitCost,
                Notes = "Auto-generated: General rehab estimate based on property square footage"
            });
        }

        return lineItemDtos;
    }

    /// <summary>
    /// Provides fallback default costs when no user template exists
    /// </summary>
    private static decimal GetDefaultCostFallback(RehabLineItemType lineItemType, RehabCondition condition)
    {
        // Default costs per unit based on typical rehab scenarios
        return (lineItemType, condition) switch
        {
            (RehabLineItemType.Bedroom, RehabCondition.Cosmetic) => 500,
            (RehabLineItemType.Bedroom, RehabCondition.Moderate) => 1500,
            (RehabLineItemType.Bedroom, RehabCondition.Heavy) => 3000,

            (RehabLineItemType.Bathroom, RehabCondition.Cosmetic) => 1000,
            (RehabLineItemType.Bathroom, RehabCondition.Moderate) => 3500,
            (RehabLineItemType.Bathroom, RehabCondition.Heavy) => 8000,

            (RehabLineItemType.General, RehabCondition.Cosmetic) => 5,   // per sqft
            (RehabLineItemType.General, RehabCondition.Moderate) => 15,  // per sqft
            (RehabLineItemType.General, RehabCondition.Heavy) => 35,     // per sqft

            _ => 0
        };
    }
}