using DealEvaluator.Domain.Entities;

namespace DealEvaluator.Application.Interfaces;

public interface IPropertyRepository : IRepository<Property>
{
    public Task<List<Property>> GetPropertiesByUserIdAsync(string userId);
    public Task<List<Property>> GetPropertiesByZipCodeAsync(string zipCode);
    public Task<List<Property>> GetPropertiesInPriceRangeAsync(decimal min, decimal max);
    public Task<List<Comparable>> GetComparablesAsync(int propertyId);
    public Task<Property?> GetPropertyByAddressAndUserAsync(string address, string userId);
}