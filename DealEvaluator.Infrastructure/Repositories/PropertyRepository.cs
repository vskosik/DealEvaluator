using DealEvaluator.Application.Interfaces;
using DealEvaluator.Domain.Entities;
using DealEvaluator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DealEvaluator.Infrastructure.Repositories;

public class PropertyRepository : DbRepository<Property>, IPropertyRepository
{
    private readonly DealEvaluatorContext _context;
    private readonly DbSet<Property> _properties;
    private readonly DbSet<Comparable> _comparables;

    public PropertyRepository(DealEvaluatorContext context) : base(context)
    {
        _context = context;
        _properties = _context.Properties;
        _comparables = _context.Comparables;
    }

    // Get properties by user id
    public async Task<List<Property>> GetPropertiesByUserIdAsync(string userId)
    {
        return await _properties
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }
    
    // Get properties by zip code
    public async Task<List<Property>> GetPropertiesByZipCodeAsync(string zipCode)
    {
        return await _properties
            .Where(p => p.ZipCode == zipCode)
            .ToListAsync();
    }

    // Get properties in price range
    public async Task<List<Property>> GetPropertiesInPriceRangeAsync(decimal min, decimal max)
    {
        return await _properties
            .Where(p => p.Price >= min && p.Price <= max)
            .ToListAsync();
    }

    // Get comparables by property id
    public async Task<List<Comparable>> GetComparablesAsync(int propertyId)
    {
        return await _comparables
            .Where(c => c.PropertyId == propertyId)
            .ToListAsync();
    }

    // Find property by address and user to prevent duplicates
    public async Task<Property?> GetPropertyByAddressAndUserAsync(string address, string userId)
    {
        return await _properties
            .FirstOrDefaultAsync(p =>
                p.Address == address &&
                p.UserId == userId);
    }
}