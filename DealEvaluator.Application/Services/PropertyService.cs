using AutoMapper;
using DealEvaluator.Application.DTOs.Comparable;
using DealEvaluator.Application.DTOs.Evaluation;
using DealEvaluator.Application.DTOs.Property;
using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Services;

/// <summary>
/// Service layer for property business logic
/// This is where your evaluation algorithms and business rules live
/// </summary>
public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IMapper _mapper;

    public PropertyService(
        IPropertyRepository propertyRepository,
        IEvaluationRepository evaluationRepository,
        IMapper mapper)
    {
        _propertyRepository = propertyRepository;
        _evaluationRepository = evaluationRepository;
        _mapper = mapper;
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

        await _propertyRepository.AddAsync(property);
        await _propertyRepository.SaveChangesAsync();

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

    // TODO: Make an actual evaluation logic
    public async Task<object> EvaluatePropertyDealAsync(int propertyId)
    {
        // 1. Get the property from database
        // var property = await _propertyRepository.GetByIdAsync(propertyId);
        // if (property == null)
        //     throw new KeyNotFoundException($"Property with ID {propertyId} not found");

        // 2. Fetch comparable properties (comps) from your repository or Zillow API
        // var comps = await _comparableRepository.GetComparablesByLocationAsync(property.ZipCode);

        // 3. Calculate ARV (After Repair Value)
        // var arv = CalculateARV(property, comps);

        // 4. Estimate repair costs based on condition
        // var repairCost = EstimateRepairCost(property);

        // 5. Calculate investment metrics:
        //    - Cap Rate = (Net Operating Income / Property Price) * 100
        //    - Cash on Cash Return
        //    - ROI
        //    - Monthly cash flow

        // 6. Save evaluation to database
        // var evaluation = new Evaluation { PropertyId = propertyId, Arv = arv, ... };
        // await _evaluationRepository.AddAsync(evaluation);

        // 7. Return results
        return new
        {
            PropertyId = propertyId,
            Message = "Evaluation logic not yet implemented - add your formulas here!",
            // ARV = arv,
            // RepairCost = repairCost,
            // CapRate = capRate,
            // etc...
        };
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

    public async Task UpdatePropertyCoordinatesAsync(int propertyId, double latitude, double longitude)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId);
        if (property == null)
            throw new KeyNotFoundException($"Property with ID {propertyId} not found");

        property.Latitude = latitude;
        property.Longitude = longitude;

        _propertyRepository.Update(property);
        await _propertyRepository.SaveChangesAsync();
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
        int repairCost = dto.RepairCost ?? 0;

        // Calculate 70% Rule metrics
        int maxOffer = (int)(arv * 0.7) - repairCost;
        int profit = arv - repairCost - maxOffer;
        decimal? roi = maxOffer > 0 ? (decimal)profit / maxOffer * 100 : null;

        // Create evaluation entity
        var evaluation = new Evaluation
        {
            PropertyId = dto.PropertyId,
            Arv = arv,
            RepairCost = repairCost,
            PurchasePrice = dto.PurchasePrice,
            MaxOffer = maxOffer,
            Profit = profit,
            Roi = roi,
            CreatedAt = DateTime.UtcNow,
            Comparables = comparables
        };

        // Save evaluation
        await _evaluationRepository.AddAsync(evaluation);
        await _evaluationRepository.SaveChangesAsync();

        // Return DTO
        return _mapper.Map<EvaluationDto>(evaluation);
    }

    // Private helper methods for calculations
    // private decimal CalculateARV(Property property, List<Comparable> comps) { }
    // private decimal EstimateRepairCost(Property property) { }
    // private decimal CalculateCapRate(decimal noi, decimal price) { }
}