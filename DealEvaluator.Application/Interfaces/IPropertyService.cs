using DealEvaluator.Application.DTOs.Evaluation;
using DealEvaluator.Application.DTOs.Property;

namespace DealEvaluator.Application.Interfaces;

/// <summary>
/// Service for managing property evaluation business logic
/// </summary>
public interface IPropertyService
{
    /// <summary>
    /// Creates a new property and performs initial evaluation
    /// </summary>
    /// <param name="dto">Property data from user input</param>
    /// <param name="userId">ID of the user creating the property</param>
    /// <returns>Created property with ID</returns>
    Task<PropertyDto> CreatePropertyAsync(CreatePropertyDto dto, string userId);

    /// <summary>
    /// Gets a property by ID
    /// </summary>
    Task<PropertyDto?> GetPropertyByIdAsync(int id);

    /// <summary>
    /// Gets all properties for a specific user
    /// </summary>
    Task<List<PropertyDto>> GetUserPropertiesAsync(string userId);

    /// <summary>
    /// Updates an existing property
    /// </summary>
    Task<PropertyDto> UpdatePropertyAsync(UpdatePropertyDto dto);

    /// <summary>
    /// Deletes a property
    /// </summary>
    Task DeletePropertyAsync(int id);

    /// <summary>
    /// Evaluates a property deal (calculates metrics like ARV, cap rate, etc.)
    /// This is where your core business logic will go
    /// </summary>
    /// <param name="propertyId">ID of property to evaluate</param>
    /// <returns>Evaluation results</returns>
    Task<object> EvaluatePropertyDealAsync(int propertyId);

    /// <summary>
    /// Gets all evaluations for a specific property, ordered by most recent first
    /// </summary>
    /// <param name="propertyId">ID of the property</param>
    /// <returns>List of evaluations</returns>
    Task<List<EvaluationDto>> GetPropertyEvaluationsAsync(int propertyId);

    /// <summary>
    /// Gets the most recent evaluation for a property
    /// </summary>
    /// <param name="propertyId">ID of the property</param>
    /// <returns>Latest evaluation or null if none exist</returns>
    Task<EvaluationDto?> GetLatestEvaluationAsync(int propertyId);
}